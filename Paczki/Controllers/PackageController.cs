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
        public int numPackagesPerPage = 5;

        private readonly IRepository _repository;
        public PackageController(IRepository repo)
        {
            _repository = repo;
        }
        public IActionResult Index()
        {
            IEnumerable<Package> packageList = _repository.GetAllPackages().Take(numPackagesPerPage);
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
        [HttpGet("/index")]
        public IActionResult TurnPage(int pageChoice)
        {
            //int numPage = Int32.Parse(Request.Form["page-choice"]);
            //int id = Int32.Parse(Request.Form["package-id"]);

            var skip = (pageChoice - 1) * numPackagesPerPage;
            int take = numPackagesPerPage;
            
            var page = _repository.GetAllPackages().Skip(skip).Take(take).ToList();
            return View("Index",page);
        }

        // POST
        public IActionResult FilterPackages(string ShowOpen, string ShowClosed)
        {
            IEnumerable<Package> packageList = _repository.GetAllPackages();
            bool showOpen = ShowOpen == "true" ? true : false;
            bool showClosed = ShowClosed == "true" ? true : false;
            if (!showOpen)
            {
                packageList = packageList.Where(o => !o.Opened).ToList(); // only show closed
            }
            if (!showClosed)
            {
                packageList = packageList.Where(o => o.Opened).ToList();   // only show opened
            }

            return View("Index", packageList);
        }

        [HttpGet]
        public IActionResult Filter()
        {
            return View();
        }

        IEnumerable<Delivery> getTempDeliveriesDeserialized(String jsonTempDeliveries, int packageID)
        {

            if (jsonTempDeliveries == null) { return Enumerable.Empty<Delivery>(); }
            var packageFromID = _repository.GetPackage(packageID);
            var deliveryList = new List<Delivery>();
            var jsonTempDeliveriesDeserialized = JsonConvert.DeserializeObject<List<DeliveryDto>>(jsonTempDeliveries);
            foreach (DeliveryDto delivery in jsonTempDeliveriesDeserialized)
            {
                deliveryList.Add(new Delivery()
                {
                    CreationDateTime = DateTime.Now,
                    Name = delivery.Name,
                    PackageRefId = packageID,
                    Weight = delivery.Weight,
                    Package = packageFromID
                });

            }
            return deliveryList;
        }

        IEnumerable<DeliveryDtoWithId> getStaticModifiedDeliveriesDeserialized(String jsonStaticModifiedDeliveries, int packageID)
        {
            if (jsonStaticModifiedDeliveries == null) { return Enumerable.Empty<DeliveryDtoWithId>(); }
            var jsonStaticModifiedDeliveriesDeserialized = JsonConvert.DeserializeObject<List<DeliveryDtoWithId>>(jsonStaticModifiedDeliveries);
            return jsonStaticModifiedDeliveriesDeserialized;
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HandleEditDeliveries()
        {
            
            var jsonTempDeliveries = Request.Form["json-temp-deliveries"];
            var jsonStaticDeliveriesModified = Request.Form["json-static-deliveries-modified"];
            JsonConvert.DeserializeObject<List<DeliveryDto>>(jsonTempDeliveries);
            var jsonStaticDeliveriesToDelete = JsonConvert.DeserializeObject<List<int>>(Request.Form["json-static-deliveries-to-delete"]);
            if (!Int32.TryParse(Request.Form["package-id"],out int packageID))
            {
                return BadRequest();
            }
            if (jsonTempDeliveries.ToString() == null && jsonStaticDeliveriesModified.ToString() == null)
            {
                return NoContent();
            }
            _repository.CreateDeliveries(getTempDeliveriesDeserialized(jsonTempDeliveries, packageID));
            _repository.UpdateDeliveries(getStaticModifiedDeliveriesDeserialized(jsonStaticDeliveriesModified, packageID));
            _repository.DeleteDeliveries(jsonStaticDeliveriesToDelete);

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
