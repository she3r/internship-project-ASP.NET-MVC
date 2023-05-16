using Paczki.Models;
using Microsoft.AspNetCore.Mvc;
using Paczki;
using Newtonsoft.Json;
using Paczki.Dto;
using Paczki.Repositories;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
            IndexPackageContentsModelView modelView = new IndexPackageContentsModelView()
            {
                PageChoice = 1,
                ShowOpen = true,
                ShowClosed = true,
                Query = _getPage(1),
                NumPackagesOnPage = numPackagesPerPage,
                NumOfAllPackages = _repository.getNumOfPackages()
            };
            return View(modelView);
        }

        // GET
        public IActionResult Create()
        {
            return View();
        }

        private IndexPackageContentsModelView _getDefaultIndexPackageContentsModelView()
        {
            return new IndexPackageContentsModelView()
            {
                Query = _getPage(),
                NumPackagesOnPage = numPackagesPerPage,
                NumOfAllPackages = _repository.getNumOfPackages()
            };
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePackage()
        {
            var name = Request.Form["packageName"];
            if(name.ToString() != null)
            {
                Package package = new Package() { Name = name, CreationDateTime = DateTime.Now, Opened = true };
                _repository.CreatePackage(package);
            }

            return RedirectToAction("Index");
        }
        [HttpGet]
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
            if (!packageFromID.Opened)
            {
                TempData["edit_operation"] = "nie mozna edytowac zamknietej paczki. Otworz paczke aby kontynuowac.";
                return RedirectToAction("Index");
            }

            var modelView = new EditPackageContentsModelView() { Package = packageFromID,
                Query = _repository.GetPackageDeliveries(id)
            };
            return View(modelView);
        }

        private IEnumerable<Package> _getPage(int pageChoice=1) {
            var skip = (pageChoice - 1) * numPackagesPerPage;
            int take = numPackagesPerPage;

            return _repository.GetAllPackages().Skip(skip).Take(take).ToList();
        }

        public IActionResult TurnPage(int pageChoice, string ShowOpen, string ShowClosed)
        {
            bool showOpen = ShowOpen == "true" ? true : false;
            bool showClosed = ShowClosed == "false" ? true : false;
            IndexPackageContentsModelView modelView = new IndexPackageContentsModelView()
            {
                PageChoice = pageChoice,
                ShowOpen = showOpen,
                ShowClosed = showClosed,
                Query = _getPage(pageChoice),
                NumPackagesOnPage = numPackagesPerPage,
                NumOfAllPackages = _repository.getNumOfPackages()
            };

            return View("Index", modelView);
        }

        [HttpGet]
        public IActionResult FilterPackages(int pageChoice, string ShowOpen, string ShowClosed)
        {
            IEnumerable<Package> packageList = _getPage(pageChoice);
            bool showOpen = ShowOpen == "true" ? true : false;
            bool showClosed = ShowClosed == "false" ? true : false;
            int numOfPackages = _repository.getNumOfPackages();
            if (!showOpen)
            {
                packageList = packageList.Where(o => !o.Opened).ToList(); // only show closed
            }
            if (!showClosed)
            {
                packageList = packageList.Where(o => o.Opened).ToList();   // only show opened
            }
            IndexPackageContentsModelView modelView = new IndexPackageContentsModelView()
            {
                PageChoice = pageChoice,
                ShowOpen = showOpen,
                ShowClosed = showClosed,
                Query = packageList,
                NumPackagesOnPage = numPackagesPerPage,
                NumOfAllPackages = numOfPackages
            };


            return View("Index", modelView);
        }

        [HttpGet]
        public IActionResult Filter()
        {
            return View();
        }
        [HttpGet]
        public IActionResult EditPackageOnly(int? id)
        {
            if (id == null)
                return BadRequest();
            var packageFromId = _repository.GetPackage(id);
            return View(packageFromId);
        }

        private int _getPageFromPositionInDb(int position)
        {
            var postsPerPage = numPackagesPerPage;
            var numItTable = (position + 1) % postsPerPage;
            var totalNumOfPackages = _repository.getNumOfPackages();
            var page = (_repository.getNumOfPackages() - numItTable) / postsPerPage;
            return page + 1;
        }
        [HttpPost]
        public IActionResult EditPackageOnly(int? id, string packageName, string packageCity)
        {
            if (id == null)
                return BadRequest();
            var packageFromId = _repository.GetPackage(id);
            string packageNameSubmit = packageName != null ? packageName : packageFromId.Name;
            string packageCitySubmit = packageCity != null ? packageCity : packageFromId.DestinationCity;
            var toEdit = new PackageDtoWithId()
            {
                Id = (int)id,
                Name = packageNameSubmit,
                DestinationCity = packageCitySubmit,
                IsOpened = packageFromId.Opened
            };
            _repository.UpdatePackage(toEdit);
            string showOpen = toEdit.IsOpened ? "true" : "false";
            string showClosed = toEdit.IsOpened ? "false" : "true";
            int page = _getPageFromPositionInDb(_repository.getPackagePosition(id));
            return RedirectToAction("TurnPage", new { pageChoice = page, ShowOpen = showOpen, ShowClosed = showClosed });
        }
        public IActionResult Delete(int? id)
        {
            if (id == null)
                return BadRequest();
            _repository.DeletePackage(id);
            return RedirectToAction("Index");
        }

        public IActionResult Open(int? id)
        {
            if(id == null)
                    return BadRequest();
            var packageFromId = _repository.GetPackage(id);
            var toUpdate = new PackageDtoWithId()
            {
                Id = packageFromId.PackageId,
                Name = packageFromId.Name,
                IsOpened=true,
                DestinationCity = packageFromId.DestinationCity
            };
            _repository.UpdatePackage(toUpdate);
            return RedirectToAction("Index");
        }

        public IActionResult Close(int? id) {
            if (id == null)
                return BadRequest();
            var packageFromId = _repository.GetPackage(id);
            var toUpdate = new PackageDtoWithId()
            {
                Id = packageFromId.PackageId,
                Name = packageFromId.Name,
                IsOpened = false,
                DestinationCity = packageFromId.DestinationCity
            };
            _repository.UpdatePackage(toUpdate);
            return RedirectToAction("Index");
        }

        private IEnumerable<Delivery> _getTempDeliveriesDeserialized(String jsonTempDeliveries, int packageID)
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

        private IEnumerable<DeliveryDtoWithId> _getStaticModifiedDeliveriesDeserialized(String jsonStaticModifiedDeliveries, int packageID)
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
            _repository.CreateDeliveries(_getTempDeliveriesDeserialized(jsonTempDeliveries, packageID));
            _repository.UpdateDeliveries(_getStaticModifiedDeliveriesDeserialized(jsonStaticDeliveriesModified, packageID));
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
