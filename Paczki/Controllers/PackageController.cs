using Paczki.Models;
using Microsoft.AspNetCore.Mvc;
using Paczki;
using Newtonsoft.Json;
using Paczki.Dto;
using Paczki.Repositories;

namespace Paczki.Controllers
{
    public class PackageController : Controller
    {

        private readonly IRepository _repository;
        public PackageController(IRepository repo)
        {
            _repository = repo;
        }
        public IActionResult Index()
        {
            IEnumerable<Package> packageList = _repository.GetAllPackages();
            return View(packageList);
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
            if(name.ToString() == null)
            {
                return RedirectToAction("Index");
            }
            Package package = new Package() { Name=name,CreationDateTime=DateTime.Now,Opened=true};
            _repository.CreatePackage(package);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int? id)
        {
            if (id == null | id == 0)
            {
                return NotFound();
            }
            var packageFromID = _repository.GetPackage(id);

            if (packageFromID == null)
            {
                return NotFound();
            }
            var modelView = new EditPackageContentsModelView() { Package = packageFromID,
                Query = _repository.GetPackageDeliveries(id)
            };
            return View(modelView);
        }

        //public IActionResult TurnPage()
        //{
        //    int numPage = Int32.Parse(Request.Form["page-choice"]);
        //    int id = Int32.Parse(Request.Form["package-id"]);
        //    if (id == null | id == 0)
        //    {
        //        return NotFound();
        //    }
        //    var packageFromID = _db.Packages.Find(id);

        //    if (packageFromID == null)
        //    {
        //        return NotFound();
        //    }
        //    IEnumerable<Delivery> objDeliveryList = _db.Deliveries.Where(d => d.PackageRefId == id)
        //        .ToList();

        //    int pageSize = 10;
        //    var skip = (numPage - 1) * pageSize;
        //    int take = pageSize;

        //    var pageOfResults = objDeliveryList.Skip(skip).Take(take).ToList();

        //    var modelView = new EditPackageContentsModelView()
        //    {
        //        Package = packageFromID,
        //        Query = _db.Deliveries.Where(d => d.PackageRefId == id).ToList()
        //    };
        //    return View(modelView);
        //}


        public IActionResult Filter()
        {
            IEnumerable<Package> packageList = _repository.GetAllPackages();
            bool ShowOpen = Request.Form["ShowOpen"].ToString() == "true" ? true : false;
            bool ShowClosed = Request.Form["ShowClosed"].ToString() == "true" ? true : false ;
            if(!ShowOpen)
            {
                packageList = packageList.Where(o => !o.Opened).ToList(); // only show closed
            }
            if(!ShowClosed)
            {
                packageList = packageList.Where(o => o.Opened).ToList();   // only show opened
            }

            return View("Index", packageList);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateDelivery()
        {
            var jsonTempDeliveries = Request.Form["json-temp-deliveries"];
            if (!Int32.TryParse(Request.Form["package-id"],out int packageID))
            {
                return BadRequest();
            }
            if (jsonTempDeliveries.ToString() == null)
            {
                return NoContent();
            }
            var jsonDeserialized = JsonConvert.DeserializeObject<List<DeliveryDto>>(jsonTempDeliveries);
            IEnumerable<Delivery> newDeliveries = new List<Delivery>();
            foreach (DeliveryDto delivery in jsonDeserialized)
            {
                _repository.CreateDelivery(
                    new Delivery()
                    {
                        CreationDateTime = DateTime.Now,
                        Name = delivery.Name,
                        PackageRefId = packageID,
                        Weight=delivery.Weight
                    }
                    );
                    
            }
            var packageFromID = _repository.GetPackage(packageID);
            var modelView = new EditPackageContentsModelView()
            {
                Package = packageFromID,
                Query = _repository.GetPackageDeliveries(packageID)
            };
            return View("Edit", modelView);


        }
    }
}
