using ItemProcessingSystemCore.Models;

namespace ItemProcessingSystemCore.Helpers
{
    public static class CycleDetector
    {
        public static bool WouldCreateCycle(int parentId, int childId, List<ItemRelation> relations)
        {
            if (parentId == childId) return true;
            return IsAncestor(childId, parentId, relations, new HashSet<int>());
        }

        private static bool IsAncestor(int nodeId, int suspectedAncestor, List<ItemRelation> relations, HashSet<int> visited)
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
    }
}
