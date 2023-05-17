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

            return View(GetDefaultIndexPackageContentsModelView());
        }
        public IActionResult IndexWithPage(int numPage)
        {
            return View("Index",new IndexPackageContentsModelView()
            {
                Query = GetPage(numPage),
                ShowOpen = true,
                ShowClosed = false,
                PageChoice = numPage,
                NumOfAllPackages = _repository.GetNumOfPackages(),
                NumPackagesOnPage = numPackagesPerPage
            });
        }

        // GET
        public IActionResult Create()
        {
            return View();
        }

        private IndexPackageContentsModelView GetDefaultIndexPackageContentsModelView()
        {
            return new IndexPackageContentsModelView()
            {
                PageChoice = 1,
                ShowOpen = true,
                ShowClosed = true,
                Query = GetPage(1),
                NumPackagesOnPage = numPackagesPerPage,
                NumOfAllPackages = _repository.GetNumOfPackages()
            };
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePackage(IndexPackageContentsModelView modelView)
        {
            string? name = modelView.NewPackageName;
            if(name is null | name == "")
            {
                modelView.NewPackageName = null;
                modelView.NewPackageCity = null;
                return RedirectToAction("Index", modelView);
            }
            string? city = modelView.NewPackageCity;
            if(city is null | city == "")
            {
                city = "";
            }
            int packageId = _repository.CreatePackage(new Package() 
            { Name = name, CreationDateTime = DateTime.Now, Opened = true,
                DestinationCity = city });
            var page = GetPackagePageNumByID(packageId);
            return RedirectToAction("TurnPage", new {pageChoice = page, ShowOpen = modelView.ShowOpen, 
                ShowClosed = modelView.ShowClosed});
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
                TempData["edit_operation"] = "nie mozna edytowac zamknietej paczki. " +
                    "Otworz paczke aby kontynuowac.";
                return RedirectToAction("Index");
            }

            var modelView = new EditPackageContentsModelView() { Package = packageFromID,
                Query = _repository.GetPackageDeliveries(id)
            };
            return View(modelView);
        }

        private IEnumerable<Package> GetPage(int pageChoice=1) {
            var skip = (pageChoice - 1) * numPackagesPerPage;
            int take = numPackagesPerPage;

            return _repository.GetAllPackages().Skip(skip).Take(take).ToList();
        }
        [HttpGet]
        public IActionResult TurnPage(int pageChoice, string showOpen, string showClosed)
        {
            bool _showOpen = showOpen == "true";
            bool _showClosed = showClosed == "false";
            IndexPackageContentsModelView modelView = new IndexPackageContentsModelView()
            {
                PageChoice = pageChoice,
                ShowOpen = _showOpen,
                ShowClosed = _showClosed,
                Query = GetPage(pageChoice),
                NumPackagesOnPage = numPackagesPerPage,
                NumOfAllPackages = _repository.GetNumOfPackages(),
                NewPackageCity=null,
                NewPackageName=null
            };

            return View("Index", modelView);
        }

        private bool EvalBoolString(string booleanValue)
        {
            if (booleanValue == "True") return true;
            if (booleanValue == "False") return false;
            throw new ArgumentException("not true or false");
        }
        [HttpGet]
        public IActionResult FilterPackages(string pageChoice, string showOpen, string showClosed)
        {
            int PageChoice;
            try
            {
                PageChoice = Int32.Parse(pageChoice);
            }
            catch(FormatException e)
            {
                PageChoice = 1;
            }
            var query = GetPage(PageChoice);
            bool _showOpen = EvalBoolString(showOpen);
            bool _showClosed = EvalBoolString(showClosed);
            if (!_showOpen)
            {
                query = query.Where(o => !o.Opened).ToList(); // only show closed
            }
            if (!_showClosed)
            {
                query = query.Where(o => o.Opened).ToList();   // only show opened
            }
            IndexPackageContentsModelView view = new IndexPackageContentsModelView()
            {
                Query = query,
                ShowOpen = _showOpen,
                ShowClosed = _showClosed,
                PageChoice = PageChoice,
                NumOfAllPackages = _repository.GetNumOfPackages(),
                NumPackagesOnPage = numPackagesPerPage,
                NewPackageCity = null,
                NewPackageName = null
            };
            return View("Index",view);
        }

        [HttpGet]
        public IActionResult Filter()
        {
            return View();
        }


        private int GetPageFromPositionInDb(int position)
        {
            var postsPerPage = numPackagesPerPage;
            var numItTable = (position + 1) % postsPerPage;
            var totalNumOfPackages = _repository.GetNumOfPackages();
            var page = (_repository.GetNumOfPackages() - numItTable) / postsPerPage;
            return page + 1;
        }

        private int GetPackagePageNumByID(int id)
        {
            int pos = _repository.GetPackagePosition(id);
            int numOnPage = pos % numPackagesPerPage;
            int numPage = (_repository.GetNumOfPackages() - numOnPage) / numPackagesPerPage;
            return numPage;
        }

        private IEnumerable<Package> GetPackagePageByID(int id)
        {
            var numPage = GetPackagePageNumByID(id);
            return GetPage(numPage);
        }

        [HttpPost]
        public IActionResult EditPackageOnly(EditPackageContentsModelView modelView)
        {
            var package = modelView.Package;
            if(package is null)
            {
                TempData["edit-result"] = "Nie udało się edytować paczki";
            }
            var id = package.PackageId;
            var toEdit = new PackageDtoWithId()
            {
                Id = id,
                Name = modelView.NewPackageName,
                DestinationCity = modelView.NewPackageCity,
                IsOpened = package.Opened
            };
            _repository.UpdatePackage(toEdit);
            int page = GetPageFromPositionInDb(_repository.GetPackagePosition(id));
            return View("Edit", id);
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
