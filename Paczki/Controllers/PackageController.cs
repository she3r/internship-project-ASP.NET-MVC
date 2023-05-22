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
                NumOfAllPackages = _repository.GetNumOfPackages(true,true)
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
            
            return RedirectToAction("TurnPage", modelView);
        }
    
        [HttpGet]
        public IActionResult Edit(IndexPackageContentsModelView prevModelView, int? id)
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
                return RedirectToAction("Index");
            }
            IEnumerable<Delivery> query = _repository.GetPackageDeliveries(id).ToList();
            var modelView = new EditPackageContentsModelView() {
                Package = packageFromID,
                Query = query.Select(d => d.AsDeliveryDtoWithId()).ToList(),
                SourceIndexPageNum = prevModelView.PageChoice,
                SourceShowClosedPage = prevModelView.ShowClosed,
                SourceShowOpenedPage = prevModelView.ShowOpen
            };
            return View(modelView);
        }

        private List<Package> GetPage(int pageChoice=1, bool showOpened=true, bool showClosed=true) {
            if (pageChoice < 1) throw new ArgumentException("page too low");
            var skip = (pageChoice - 1) * numPackagesPerPage;
            int take = numPackagesPerPage;
            var page = _repository.GetAllPackages();
            if (!showOpened)
            {
                page = page.Where(item => item.Opened ==  false);
            }
            if (!showClosed)
            {
                page = page.Where(item => item.Opened ==  true);
            }
            
            page = page.Skip(skip).Take(take);
            if (page.Count() < numPackagesPerPage && pageChoice > 1)
            {
                return GetPage(pageChoice - 1, showOpened, showClosed);
            }
            return page.ToList();
        }

        private int GetNumOfAllPages(bool showOpened = true, bool showClosed = true)
        {
            // num of all pages with given filters
            var page = _repository.GetAllPackages();
            if (!showOpened)
            {
                page = page.Where(item => item.Opened == false);
            }
            if (!showClosed)
            {
                page = page.Where(item => item.Opened == true);
            }
            return page.Count();
        }
        [HttpGet]
        public IActionResult TurnPage(IndexPackageContentsModelView modelView)
        {
            modelView.Query = GetPage(modelView.PageChoice, modelView.ShowOpen,modelView.ShowClosed);
            return View("Index", modelView);
        }

        [HttpGet]
        public IActionResult FilterPackages(IndexPackageContentsModelView modelView)
        {
            modelView.Query = GetPage(modelView.PageChoice, modelView.ShowOpen, modelView.ShowClosed);
            modelView.NumOfAllPackages = _repository.GetNumOfPackages(modelView.ShowOpen, modelView.ShowClosed);
            modelView.NumPackagesOnPage = numPackagesPerPage;

            return View("Index", modelView);
        }

        [HttpGet]
        public IActionResult GoBack(int id, EditPackageContentsModelView modelView)
        {
            return RedirectToAction("TurnPage", new IndexPackageContentsModelView()
            {
                PageChoice = GetPackagePageNumByID(id),
                Query = GetPage(GetPackagePageNumByID(id)),
                ShowOpen = true,
                ShowClosed = true,
                NumOfAllPackages = GetNumOfAllPages(),
                NumPackagesOnPage = numPackagesPerPage

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

        //private IEnumerable<Package> GetPackagePageByID(int id)
        //{
        //    var numPage = GetPackagePageNumByID(id);
        //    return GetPage(numPage);
        //}

        //[HttpPost]
        //public IActionResult EditPackageOnly(EditPackageContentsModelView modelView)
        //{
        //    var package = modelView.Package;
        //    if(package is null)
        //    {
        //        TempData["edit-result"] = "Nie udało się edytować paczki";
        //    }
        //    var id = package.PackageId;
        //    var toEdit = new PackageDtoWithId()
        //    {
        //        Id = id,
        //        Name = modelView.NewPackageName,
        //        DestinationCity = modelView.NewPackageCity,
        //        IsOpened = package.Opened
        //    };
        //    _repository.UpdatePackage(toEdit);
        //    int page = GetPackagePageNumByID(id);
        //    return View("Edit", id);
        //}
        public IActionResult Delete(IndexPackageContentsModelView modelView,int? id)
        {
            if (id == null)
                return BadRequest();
            _repository.DeletePackage(id);
            modelView.NumOfAllPackages = _repository.GetNumOfPackages(modelView.ShowOpen, modelView.ShowClosed);
            modelView.Query = GetPage(modelView.PageChoice, modelView.ShowOpen, modelView.ShowClosed);
            return RedirectToAction("Index", modelView);
        }

        public IActionResult Open(IndexPackageContentsModelView modelView, int? id)
        {
            if (id == null)
                return BadRequest();
            var packageFromId = _repository.GetPackage(id);
            var toUpdate = new PackageDtoWithId()
            {
                Id = packageFromId.PackageId,
                Name = packageFromId.Name,
                IsOpened = true,
                DestinationCity = packageFromId.DestinationCity
            };
            _repository.UpdatePackage(toUpdate);
            modelView.Query = GetPage(modelView.PageChoice, modelView.ShowOpen, modelView.ShowClosed);
            return View("Index", modelView);
        }

        public IActionResult Close(IndexPackageContentsModelView modelView, int? id)
        {
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
            int currPage = modelView.PageChoice;
            bool showOpen = modelView.ShowOpen;
            bool showClosed = modelView.ShowClosed;
            modelView.Query = GetPage(currPage, showOpen, showClosed);
            return View("Index", modelView);
        }

        //public IActionResult AddNewPackage()
        //{

        //}

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HandleEditDeliveries(EditPackageContentsModelView modelView)
        {
            var packageID = modelView.Package.PackageId;
            if (packageID <= 0)
            {
                return BadRequest();
            }
            if (modelView.IsPackageModified)
            {
                _repository.UpdatePackage(new PackageDtoWithId()
                {
                    Id = packageID,
                    Name = modelView.Package.Name,
                    DestinationCity = modelView.Package.DestinationCity,
                    IsOpened = true
                });
            }
            if (modelView.Query.Any(delivery => delivery.IsModified))
            {
                modelView.Query = modelView.Query.Select(delivery => { delivery.PackageRefId = modelView.Package.PackageId; return delivery; }).ToList();
                _repository.UpdateOrInsertDeliveries(modelView.Query);
            }
            modelView.Package = _repository.GetPackage(packageID);  // get freshly updated from db 
            return View("Edit", modelView);
        }

        public IActionResult AddNewDelivery(EditPackageContentsModelView modelView)
        {
            int tempId = modelView.Query.Max(delivery => delivery.Id) + 1;
            modelView.Query.Add(new DeliveryDtoWithId() { IsModified = true, CreationDateTime=DateTime.Now, Name="",Weight=0, Id= tempId  });
            return View("Edit", modelView);
        }

        public IActionResult DeleteDelivery(EditPackageContentsModelView view, int? id) {
            if (id is not null)
            {
                view.Query = view.Query.Where(delivery => delivery.Id != id).ToList();
                view.AreDeliveriesDeleted = true;
            }
            return View("Edit", view); 
        }
    }
}
