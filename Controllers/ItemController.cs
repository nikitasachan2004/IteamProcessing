using Microsoft.AspNetCore.Mvc;
using System.Text;
using ItemProcessingSystemCore.DAL;
using ItemProcessingSystemCore.Models;

namespace ItemProcessingSystemCore.Controllers
{
    public class ItemController : Controller
    {
        private readonly DbHelper _db;
        private readonly ILogger<ItemController> _logger;

        public ItemController(DbHelper db, ILogger<ItemController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                var items = _db.GetAllItems();
                return View(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load items");
                TempData["ErrorMessage"] = "Could not load items. Please try again.";
                return View(new List<Item>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Item item)
        {
            if (!ModelState.IsValid)
                return View(item);

            try
            {
                _db.InsertItem(item);
                TempData["SuccessMessage"] = "Item created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create item");
                ModelState.AddModelError("", "Something went wrong. Please try again.");
                return View(item);
            }
        }

        public IActionResult Edit(int id)
        {
            try
            {
                var item = _db.GetItemById(id);
                if (item == null)
                {
                    TempData["ErrorMessage"] = "Item not found.";
                    return RedirectToAction(nameof(Index));
                }
                return View(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load item {Id}", id);
                TempData["ErrorMessage"] = "Could not load item.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Item item)
        {
            if (!ModelState.IsValid)
                return View(item);

            try
            {
                bool updated = _db.UpdateItem(item);
                if (!updated)
                {
                    TempData["ErrorMessage"] = "Item not found.";
                    return RedirectToAction(nameof(Index));
                }
                TempData["SuccessMessage"] = "Item updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update item {Id}", item.ItemId);
                ModelState.AddModelError("", "Something went wrong. Please try again.");
                return View(item);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                bool deleted = _db.DeleteItem(id);
                TempData[deleted ? "SuccessMessage" : "ErrorMessage"] =
                    deleted ? "Item deleted." : "Item not found.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete item {Id}", id);
                TempData["ErrorMessage"] = "Something went wrong while deleting.";
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Process()
        {
            try
            {
                return View(_db.GetAllItems());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load items for Process");
                return View(new List<Item>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Process(int parentId, int[] childIds)
        {
            if (childIds == null || childIds.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select at least one child item.";
                return RedirectToAction(nameof(Process));
            }

            if (childIds.Contains(parentId))
            {
                TempData["ErrorMessage"] = "An item cannot be a child of itself.";
                return RedirectToAction(nameof(Process));
            }

            try
            {
                var existingRelations = _db.GetAllRelations();

                foreach (int childId in childIds)
                {
                    if (_db.RelationExists(parentId, childId))
                    {
                        TempData["ErrorMessage"] = "One or more of these relations already exists.";
                        return RedirectToAction(nameof(Process));
                    }

                    if (WouldCreateCycle(parentId, childId, existingRelations))
                    {
                        TempData["ErrorMessage"] = "That would create a circular dependency.";
                        return RedirectToAction(nameof(Process));
                    }

                    _db.InsertRelation(parentId, childId);
                    existingRelations.Add(new ItemRelation { ParentItemId = parentId, ChildItemId = childId });
                }

                TempData["SuccessMessage"] = $"{childIds.Length} child item(s) linked to parent.";
                return RedirectToAction(nameof(Tree));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save relations");
                TempData["ErrorMessage"] = "Something went wrong. Please try again.";
                return RedirectToAction(nameof(Process));
            }
        }

        public IActionResult Tree()
        {
            try
            {
                var items = _db.GetAllItems();
                var relations = _db.GetAllRelations();
                ViewBag.TreeHtml = BuildTreeHtml(items, relations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load tree");
                ViewBag.TreeHtml = "<p class='text-danger'>Could not load tree. Please try again.</p>";
            }
            return View();
        }

        private bool WouldCreateCycle(int parentId, int childId, List<ItemRelation> relations)
        {
            if (parentId == childId) return true;
            return IsAncestor(childId, parentId, relations, new HashSet<int>());
        }

        private bool IsAncestor(int nodeId, int suspectedAncestor, List<ItemRelation> relations, HashSet<int> visited)
        {
            if (visited.Contains(nodeId)) return false;
            visited.Add(nodeId);

            foreach (var pid in relations.Where(r => r.ChildItemId == nodeId).Select(r => r.ParentItemId))
            {
                if (pid == suspectedAncestor) return true;
                if (IsAncestor(pid, suspectedAncestor, relations, visited)) return true;
            }
            return false;
        }

        private string BuildTreeHtml(List<Item> items, List<ItemRelation> relations)
        {
            if (items.Count == 0)
                return "<p>No items yet. <a href='/Item/Create'>Create one</a>.</p>";

            var childrenMap = new Dictionary<int, List<ItemRelation>>();
            foreach (var rel in relations)
            {
                if (!childrenMap.ContainsKey(rel.ParentItemId))
                    childrenMap[rel.ParentItemId] = new List<ItemRelation>();
                childrenMap[rel.ParentItemId].Add(rel);
            }

            var childIdSet = new HashSet<int>(relations.Select(r => r.ChildItemId));
            var roots = items.Where(i => !childIdSet.Contains(i.ItemId)).ToList();

            var sb = new StringBuilder();
            sb.Append("<div class='mt-2'>");

            if (roots.Count == 0 && relations.Count > 0)
                sb.Append("<p class='text-warning'>No root nodes found — data may be inconsistent.</p>");

            foreach (var root in roots)
                sb.Append(BuildItemNode(root, childrenMap, items, new HashSet<int>()));

            var standalone = items.Where(i => !childIdSet.Contains(i.ItemId) && !childrenMap.ContainsKey(i.ItemId)).ToList();
            if (standalone.Count > 0)
            {
                sb.Append("<div class='alert alert-warning mt-3'><strong>Unlinked items (no relations):</strong><ul class='mb-0 mt-1'>");
                foreach (var item in standalone)
                    sb.Append($"<li>{Encode(item.Name)} &mdash; Weight: {item.Weight}</li>");
                sb.Append("</ul></div>");
            }

            sb.Append("</div>");
            return sb.ToString();
        }

        private string BuildItemNode(Item? item, Dictionary<int, List<ItemRelation>> childrenMap,
            List<Item> allItems, HashSet<int> visited)
        {
            if (item == null || visited.Contains(item.ItemId)) return "";
            visited.Add(item.ItemId);

            var sb = new StringBuilder();
            sb.Append("<div class='card mb-2'><div class='card-body py-2'>");
            sb.Append($"<strong>{Encode(item.Name)}</strong> <span class='text-muted'>— Weight: {item.Weight}</span>");

            if (childrenMap.TryGetValue(item.ItemId, out var children))
            {
                sb.Append("<ul class='mt-2 mb-0'>");
                foreach (var rel in children)
                {
                    var child = allItems.FirstOrDefault(i => i.ItemId == rel.ChildItemId);
                    if (child != null)
                        sb.Append($"<li>{BuildItemNode(child, childrenMap, allItems, visited)}</li>");
                }
                sb.Append("</ul>");
            }

            sb.Append("</div></div>");
            return sb.ToString();
        }

        private static string Encode(string? s) =>
            System.Net.WebUtility.HtmlEncode(s ?? "Unnamed");
    }
}