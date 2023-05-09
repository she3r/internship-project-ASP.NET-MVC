using Paczki.Models;
using Microsoft.AspNetCore.Mvc;
using Paczki;
using System.Dynamic;

namespace Paczki.Controllers
{
    public class PackageController : Controller
    {
        private readonly AppDbContext _db;
        public PackageController(AppDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            IEnumerable<Package> objCategoryList = _db.Packages.ToList();
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
        public IActionResult CreatePackage()
        {
            var name = Request.Form["packageName"];
            Package package = new Package() { Name=name,CreationDateTime=DateTime.Now,Opened=true};
            _db.Packages.Add(package);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int? id)
        {
            if (id == null | id == 0)
            {
                return NotFound();
            }
            var packageFromID = _db.Packages.Find(id);

            if (packageFromID == null)
            {
                return NotFound();
            }
            dynamic doubleModel = new ExpandoObject();
            doubleModel.Package = packageFromID;
            doubleModel.Query = _db.Deliveries.Where(d => d.PackageRefId == id).ToList();
            return View(doubleModel);
        }


        public IActionResult Filter()
        {
            IEnumerable<Package> objCategoryList = _db.Packages.ToList();
            bool ShowOpen = Request.Form["ShowOpen"].ToString() == "true" ? true : false;
            bool ShowClosed = Request.Form["ShowClosed"].ToString() == "true" ? true : false ;
            if(!ShowOpen)
            {
                objCategoryList=objCategoryList.Where(o => !o.Opened).ToList(); // only show closed
            }
            if(!ShowClosed)
            {
                objCategoryList=objCategoryList.Where(o => o.Opened).ToList();   // only show opened
            }

            return View("Index",objCategoryList);
        }
    }
}
