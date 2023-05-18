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

        private IEnumerable<Package> GetTempPackagesDeserialized(string? jsonTempPackages)
        {
            if(jsonTempPackages is null || jsonTempPackages == "") { 
                return Enumerable.Empty<Package>();
            }
            var jsonTempDeliveriesDeserialized = JsonConvert.DeserializeObject<List<PackageDtoWithId>>(jsonTempPackages);
            var packages = new List<Package>();
            foreach(var dto in jsonTempDeliveriesDeserialized)
            {
                var city = dto.DestinationCity == null ? "" : dto.DestinationCity;
                packages.Add(new Package()
                {
                    Name=dto.Name,
                    Opened=true,
                    CreationDateTime=DateTime.Now,
                    DestinationCity= city,

                });
            }
            return packages;
        }

        private IEnumerable<PackageDtoWithId> GetUpdateOpenPackagesDeserialized(string? jsonUpdatePackages)
        {
            if (jsonUpdatePackages is null || jsonUpdatePackages == "")
            {
                return Enumerable.Empty<PackageDtoWithId>();
            }
            var jsonTempDeliveriesDeserialized = JsonConvert.DeserializeObject<List<PackageDtoUpdateOpen>>(jsonUpdatePackages);
            var packages = new List<PackageDtoWithId>();
            foreach (var dto in jsonTempDeliveriesDeserialized)
            {
                packages.Add(new PackageDtoWithId()
                {
                    Id=dto.Id,
                    Name = null,
                    IsOpened = dto.IsOpened,
                    DestinationCity=null

                });
            }
            return packages;
        }

        private IEnumerable<int> GetDeletePackagesDeserialized(string? jsonDeletePackagesIDs)
        {
            if (jsonDeletePackagesIDs is null || jsonDeletePackagesIDs == "")
            {
                return Enumerable.Empty<int>();
            }
            return JsonConvert.DeserializeObject<List<int>>(jsonDeletePackagesIDs);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HandleEditPackages(IndexPackageContentsModelView modelView)
        {
            if (modelView.NewPackagesJSON is not null)
            {
                var toAddPackages = GetTempPackagesDeserialized(modelView.NewPackagesJSON);
                _repository.CreatePackages(toAddPackages);
            }
            if(modelView.ToUpdateQueryPackagesJSON is not null)
            {
                var toUpdatePackages = GetUpdateOpenPackagesDeserialized(modelView.ToUpdateQueryPackagesJSON);
                _repository.UpdatePackages(toUpdatePackages);
            }
            if (modelView.ToDeleteQueryPackageIDsJSON is not null)
            {
                var toDeletePackageIDs = JsonConvert.DeserializeObject<List<int>>(modelView.ToDeleteQueryPackageIDsJSON);
                _repository.DeletePackages(toDeletePackageIDs);
            }
            
            return RedirectToAction("TurnPage", new {pageChoice = 1, ShowOpen = modelView.ShowOpen, 
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

            var modelView = new EditPackageContentsModelView() {
                PackageId = (int) id,
                Package = packageFromID,
                Query = _repository.GetPackageDeliveries(id),

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
                NumOfAllPackages = _repository.GetNumOfPackages()
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
                NumPackagesOnPage = numPackagesPerPage
            };
            return View("Index",view);
        }

        [HttpGet]
        public IActionResult Filter()
        {
            return View();
        }
        [HttpGet]
        public IActionResult GoBack(int id, EditPackageContentsModelView modelView)
        {
            return RedirectToAction("TurnPage", new
            {
                pageChoice = GetPackagePageNumByID(id),
                ShowOpen = true,
                ShowClosed = true
            });
        }


        //private int GetPageFromPositionInDb(int position)
        //{
        //    var postsPerPage = numPackagesPerPage;
        //    var numItTable = (position + 1) % postsPerPage;
        //    var totalNumOfPackages = _repository.GetNumOfPackages();
        //    var page = (_repository.GetNumOfPackages() - numItTable) / postsPerPage;
        //    return page + 1;
        //}

        private int GetPackagePageNumByID(int id)
        {
            int pos = _repository.GetPackagePosition(id) + 1;
            int numOnPage = pos % numPackagesPerPage;
            if(numOnPage == 0) { return pos / numPackagesPerPage; }
            return pos / numPackagesPerPage + 1;
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
            int page = GetPackagePageNumByID(id);
            return View("Edit", id);
        }
        public IActionResult Delete(int? id)
        {
            if (id == null)
                return BadRequest();
            _repository.DeletePackage(id);
            return RedirectToAction("Index");
        }

        //public IActionResult Open(int? id)
        //{
        //    if(id == null)
        //            return BadRequest();
        //    var packageFromId = _repository.GetPackage(id);
        //    var toUpdate = new PackageDtoWithId()
        //    {
        //        Id = packageFromId.PackageId,
        //        Name = packageFromId.Name,
        //        IsOpened=true,
        //        DestinationCity = packageFromId.DestinationCity
        //    };
        //    _repository.UpdatePackage(toUpdate);
        //    return RedirectToAction("Index");
        //}

        //public IActionResult Close(int? id) {
        //    if (id == null)
        //        return BadRequest();
        //    var packageFromId = _repository.GetPackage(id);
        //    var toUpdate = new PackageDtoWithId()
        //    {
        //        Id = packageFromId.PackageId,
        //        Name = packageFromId.Name,
        //        IsOpened = false,
        //        DestinationCity = packageFromId.DestinationCity
        //    };
        //    _repository.UpdateClosePackage(toUpdate);
        //    return RedirectToAction("Index");
        //}

        private IEnumerable<Delivery> GetTempDeliveriesDeserialized(string? jsonTempDeliveries, int packageID)
        {

            if (jsonTempDeliveries == null) { return Enumerable.Empty<Delivery>(); }
            var packageFromID = _repository.GetPackage(packageID);
            var deliveryList = new List<Delivery>();
            var jsonTempDeliveriesDeserialized = JsonConvert.DeserializeObject<List<DeliveryDto>>(jsonTempDeliveries);
            if (jsonTempDeliveriesDeserialized != null)
            {
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
            }
            return deliveryList;
        }

        private IEnumerable<DeliveryDtoWithId> GetStaticModifiedDeliveriesDeserialized(string? jsonStaticModifiedDeliveries, int packageID)
        {
            if (jsonStaticModifiedDeliveries == null) { return Enumerable.Empty<DeliveryDtoWithId>(); }
            var jsonStaticModifiedDeliveriesDeserialized = JsonConvert.DeserializeObject<List<DeliveryDtoWithId>>(jsonStaticModifiedDeliveries);
            return jsonStaticModifiedDeliveriesDeserialized;
        }

        IEnumerable<int> GetStaticDeliveriesToDelete(string? jsonStaticDeliveriesToDelete)
        {
            var deserialized = new List<int>();
            if (jsonStaticDeliveriesToDelete != null)
            {
                deserialized = JsonConvert.DeserializeObject<List<int>>(jsonStaticDeliveriesToDelete);
            }
            return deserialized;
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HandleEditDeliveries(EditPackageContentsModelView modelView)
        {
            var packageID = modelView.PackageId;
            if (packageID <= 0)
            {
                return BadRequest();
            }
            var jsonTempDeliveries = modelView.JsonTempDeliveries;
            var jsonStaticDeliveriesModified = modelView.JsonStaticDeliveriesModified;
            var jsonStaticDeliveriesToDelete = modelView.JsonStaticDeliveriesToDelete;
            PackageDtoWithId packageDtoWithId = new PackageDtoWithId()
            {
                Id = packageID,
                Name = modelView.NewPackageName,
                DestinationCity = modelView.NewPackageCity,
                IsOpened = true
            };

            var toCreateDeliveries = GetTempDeliveriesDeserialized(jsonTempDeliveries, packageID);
            if (toCreateDeliveries != null) {
                _repository.CreateDeliveries(GetTempDeliveriesDeserialized(jsonTempDeliveries, packageID));
            }
            var toUpdateDeliveries = GetStaticModifiedDeliveriesDeserialized(jsonStaticDeliveriesModified, packageID);
            if (toUpdateDeliveries != null)
            {
                _repository.UpdateDeliveries(toUpdateDeliveries);
            }
            var toDeleteDeliveries = GetStaticDeliveriesToDelete(jsonStaticDeliveriesToDelete);
            if (toDeleteDeliveries != null)
            {
                _repository.DeleteDeliveries(toDeleteDeliveries);
            }
            _repository.UpdatePackage(packageDtoWithId);
            modelView.Query = _repository.GetPackageDeliveries(packageID);
            modelView.JsonStaticDeliveriesToDelete = "";
            modelView.JsonStaticDeliveriesModified = "";
            modelView.JsonTempDeliveries = "";
            modelView.NewPackageCity = "";
            modelView.NewPackageName = "";
            modelView.Package = _repository.GetPackage(packageID);
    
            return View("Edit", modelView);


        }
    }
}
