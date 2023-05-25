using Paczki.Models;
using Microsoft.AspNetCore.Mvc;
using Paczki;
using Newtonsoft.Json;
using Paczki.Dto;
using Paczki.Repositories;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Data.Entity.Infrastructure.Design.Executor;

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

        public IActionResult IndexWithPage(int numPage, bool showOpen=true, bool showClosed=true)
        {
            var query = GetPage(numPage, showOpen, showClosed);
            while((query is null || query.Count == 0) && numPage > 1)
            {
                query = GetPage(numPage - 1, showOpen, showClosed);
                numPage--;
            }
            return View("Index",new IndexPackageContentsModelView()
            {
                Query = query,
                ShowOpen = showOpen,
                ShowClosed = showClosed,
                PageChoice = numPage,
                NumOfAllPackages = _repository.GetNumOfPackages(showOpen,showClosed),
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
            List<Delivery> query = _repository.GetPackageDeliveries(id).ToList();
            var queryDto = query.Select(d => d.AsDeliveryDtoWithId()).ToList();
            //queryDto.Add(new DeliveryDtoWithId() { Id = -1, Name = "" });
            var modelView = new EditPackageContentsModelView() {
                Package = packageFromID,
                Query = queryDto,
                SourceIndexPageNum = prevModelView.PageChoice,
                SourceShowClosedPage = prevModelView.ShowClosed,
                SourceShowOpenedPage = prevModelView.ShowOpen
            };

            return View(modelView);
        }
        [HttpGet]
        public IActionResult AddNewPackage(IndexPackageContentsModelView prevModelView, int id)
        {
            var packageToAdd = new Package()
            {
                PackageId = -1,
                Name = ""
            };
            if (!prevModelView.ShowOpen)
            {
                id = 1;
            }
            var listOfDeliveriesWithOneDeliveryInputForStarters = new List<DeliveryDtoWithId>();
            //listOfDeliveriesWithOneDeliveryInputForStarters.Add(new DeliveryDtoWithId() { Id = -1 });
            var modelView = new EditPackageContentsModelView()
            {
                Package = packageToAdd,
                Query = listOfDeliveriesWithOneDeliveryInputForStarters,
                SourceIndexPageNum = id,
                SourceShowClosedPage = prevModelView.ShowClosed,
                SourceShowOpenedPage = prevModelView.ShowOpen
            };
            return View("Edit", modelView);
        }

        [HttpGet]
        public IActionResult AfterEdit(bool sourceShowOpenedPage, bool sourceShowClosedPage, int sourceIndexPageNum, int id)
        {
            IEnumerable<Delivery> query = _repository.GetPackageDeliveries(id).ToList();
            var packageFromID = _repository.GetPackage(id);
            var queryDto = query.Select(d => d.AsDeliveryDtoWithId()).ToList();
            //queryDto.Add(new DeliveryDtoWithId() { Name="", Id= -1 });
            var modelView = new EditPackageContentsModelView()
            {
                Package = packageFromID,
                Query = queryDto,
                SourceIndexPageNum = sourceIndexPageNum,
                SourceShowClosedPage = sourceShowClosedPage,
                SourceShowOpenedPage = sourceShowOpenedPage
            };
            return View("Edit",modelView);
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
            //modelView.Query = GetPage(modelView.PageChoice, modelView.ShowOpen,modelView.ShowClosed);
            return RedirectToAction("IndexWithPage", new { numPage=modelView.PageChoice, showOpen=modelView.ShowOpen, showClosed=modelView.ShowClosed });
        }

        [HttpGet]
        public IActionResult FilterPackages(IndexPackageContentsModelView modelView)
        {
            return RedirectToAction("TurnPage", modelView);
        }

        [HttpGet]
        public IActionResult GoBack(int id, EditPackageContentsModelView modelView)
        {
            return RedirectToAction("TurnPage", new IndexPackageContentsModelView()
            {
                PageChoice = modelView.SourceIndexPageNum,
                Query = GetPage(modelView.SourceIndexPageNum, modelView.SourceShowOpenedPage, modelView.SourceShowClosedPage),
                ShowOpen = modelView.SourceShowOpenedPage,
                ShowClosed = modelView.SourceShowClosedPage,
                NumOfAllPackages = GetNumOfAllPages(modelView.SourceShowOpenedPage, modelView.SourceShowClosedPage),
                NumPackagesOnPage = numPackagesPerPage

            });
        }

        //private int GetPackagePageNumByID(int id)
        //{
        //    int pos = _repository.GetPackagePosition(id) + 1;
        //    int numOnPage = pos % numPackagesPerPage;
        //    if(numOnPage == 0) { return pos / numPackagesPerPage; }
        //    return pos / numPackagesPerPage + 1;
        //}
        [HttpPost]
        public IActionResult Delete(IndexPackageContentsModelView modelView,int? id)
        {
            if (id == null)
                return BadRequest();
            _repository.DeletePackage(id);
            int currPage = modelView.PageChoice;
            bool showOpen = modelView.ShowOpen;
            bool showClosed = modelView.ShowClosed;
            return RedirectToAction("IndexWithPage", new { numPage = currPage, showOpen = showOpen, showClosed = showClosed });
        }
        [HttpPost]
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
            int currPage = modelView.PageChoice;
            bool showOpen = modelView.ShowOpen;
            bool showClosed = modelView.ShowClosed;
            return RedirectToAction("IndexWithPage", new { numPage = currPage, showOpen = showOpen, showClosed = showClosed });
        }
        [HttpPost]
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
            return RedirectToAction("IndexWithPage", new {numPage=currPage, showOpen = showOpen, showClosed = showClosed});
        }

        IList<DeliveryDtoWithId> GetDeliveriesToUpdateOrInsert(IEnumerable<DeliveryDtoWithId> deliveryList, int parentPackageId)
        {
            if(deliveryList != null)
            {
                return deliveryList.Where(delivery => delivery.IsModified).
                    Select(delivery => { delivery.PackageRefId = parentPackageId; delivery.CreationDateTime = DateTime.Now; return delivery; }).ToList();
            }
            return new List<DeliveryDtoWithId>();
        }

        IList<DeliveryDtoWithId> GetDeliveriesToDelete(IEnumerable<DeliveryDtoWithId> deliveryList, int parentPackageId)
        {
            if(deliveryList != null)
            {
                return deliveryList.Where(delivery => delivery.IsDeleted).
                        Select(delivery => { delivery.PackageRefId = parentPackageId; return delivery; }).ToList();
            }
            return new List<DeliveryDtoWithId>();
        }

        List<DeliveryDtoWithId> ValidateDeliveriesToHandle(IList<DeliveryDtoWithId> deliveryList)
        {
            if (deliveryList != null)
            {
                return deliveryList.Where(d => d.Weight > 0 && d.Name != null && d.Name != "").ToList();
            }
            return new List<DeliveryDtoWithId>();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HandleEditDeliveries(EditPackageContentsModelView modelView)
        {
            int? packageID = modelView.Package?.PackageId;
            if (packageID == null || packageID == 0 || modelView.Package?.Name == "" || modelView.Package?.Name is null)    // package name cannot be null or empty ""
            {
                return BadRequest();
            }
            modelView.Query = ValidateDeliveriesToHandle(modelView.Query);
            if (packageID < 0)   // is the package new
            {
                var destinationCity = modelView.Package.DestinationCity == null ? "" : modelView.Package.DestinationCity;
                var toCreatePackage = new Package()
                {
                    Name = modelView.Package.Name,
                    DestinationCity = destinationCity
                };
                packageID = _repository.CreatePackage(toCreatePackage);
            }
            if (modelView.IsPackageModified)
            {
                var toUpdatePackage = new PackageDtoWithId()
                {
                    Id = (int) packageID,
                    Name = modelView.Package.Name,
                    DestinationCity = modelView.Package.DestinationCity,
                    IsOpened = true,
                };
                _repository.UpdatePackage(toUpdatePackage);
            }
            if (modelView.Query.Any(delivery => delivery.IsDeleted))
            {
                var ToDelete = GetDeliveriesToDelete(modelView.Query, (int) packageID);
                _repository.DeleteDeliveries(ToDelete);
                modelView.Query = modelView.Query.Where(delivery => delivery.IsDeleted == false).ToList();
            }
            if (modelView.Query.Any(delivery => delivery.IsModified))
            {
                var toUpdate = GetDeliveriesToUpdateOrInsert(modelView.Query, (int) packageID);
                _repository.UpdateOrInsertDeliveries(toUpdate);
            }
            return RedirectToAction("AfterEdit", new {
                sourceShowOpenedPage = modelView.SourceShowOpenedPage,
                sourceShowClosedPage = modelView.SourceShowClosedPage,
                sourceIndexPageNum = modelView.SourceIndexPageNum, id = (int) packageID
            });
        }



        [HttpPost]
        public IActionResult AddNewDelivery(EditPackageContentsModelView modelView)
        {
            int emptyDeliveryLimit = 10;
            if(modelView.Query is null)
            {
                modelView.Query = new List<DeliveryDtoWithId>();
            }
            // get rid of non-empty deliveries
            modelView.Query = modelView.Query.Where(delivery => (delivery.Id != -1 || (delivery.Name != null && delivery.Name != "" && delivery.Weight != 0))).ToList();
            if (modelView.CountEmptyDeliveries >= emptyDeliveryLimit)
            {
                TempData["AddNewEmptyDelivery"] = "Limit pustych przesylek to " + emptyDeliveryLimit.ToString();
                return View("Edit", modelView);
            }
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
