using Microsoft.AspNetCore.Mvc;
using ItemProcessingSystemCore.DAL;

namespace ItemProcessingSystemCore.Controllers
{
    public class TestController : Controller
    {
        private readonly DbHelper _db;

        public TestController(DbHelper db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            try
            {
                using var conn = _db.OpenConnection();
                return Content("DB connection OK");
            }
            catch (Exception ex)
            {
                return Content($"DB connection failed: {ex.Message}");
            }
        }
    }
}