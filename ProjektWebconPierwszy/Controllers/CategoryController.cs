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
            TempData["success"] = "Category created sucessfully";
            return View(obj);
        }

        public IActionResult Edit(int? id)
        {
            if(id==null | id == 0)
            {
                return NotFound();
            }
            var categoryFromDb = _db.Categories.Find(id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The Display cannot match the name");
            }

            if (ModelState.IsValid)
            {
                _db.Categories.Update(obj);
                _db.SaveChanges();  // musi byc bo sie nie zapisze
                return RedirectToAction("Index");
            }
            TempData["success"] = "Category edited sucessfully";
            return View(obj);
        }

        public IActionResult Remove(int? id)
        {
            if (id == null | id == 0)
            {
                return NotFound();
            }
            var categoryFromDb = _db.Categories.Find(id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemovePOST(int? id)
        {
            var obj = _db.Categories.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Categories.Remove(obj);
            _db.SaveChanges();
            TempData["success"] = "Category removed sucessfully";
            return RedirectToAction("Index");
        }
    }
}
