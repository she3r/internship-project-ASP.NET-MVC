using Microsoft.AspNetCore.Mvc;
using ProjektWebconPierwszy.Data;
using ProjektWebconPierwszy.Models;

namespace ProjektWebconPierwszy.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDbContext _db;
        public CategoryController(AppDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            IEnumerable<Category> objCategoryList=_db.Categories.ToList();
            return View(objCategoryList);
        }
        // GET
        public IActionResult Create()
        {
            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {   
            if(obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name","The Display cannot match the name");
            }
            
            if (ModelState.IsValid)
            {
                _db.Categories.Add(obj);
                _db.SaveChanges();  // musi byc bo sie nie zapisze
                return RedirectToAction("Index");
            }
            return View(obj);
        }
    }
}
