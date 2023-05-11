using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Paczki.Dto;
using Paczki.Models;

namespace Paczki.Controllers
{
    public class DeliveryController : Controller
    {
        private readonly AppDbContext _db;
        public DeliveryController(AppDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }



        // POST
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Create()
        //{
        //    var jsonTempDeliveries = Request.Form["json-temp-deliveries"];
        //    var packageID = Int32.Parse(Request.Form["package-id"]);
        //    if (jsonTempDeliveries.ToString() == null)
        //    {
        //        return NoContent();
        //    }
        //    if (packageID.ToString() == null)
        //    {
        //        return BadRequest();
        //    }
        //    var jsonDeserialized = JsonConvert.DeserializeObject<List<DeliveryDto>>(jsonTempDeliveries);
        //    IEnumerable<Delivery> newDeliveries = new List<Delivery>();
        //    foreach(DeliveryDto delivery in jsonDeserialized)
        //    {
        //        _db.Deliveries.Add(new Delivery() { CreationDateTime=DateTime.Now, 
        //            Name= delivery.Name, PackageRefId=packageID });
        //    }
        //    var packageFromID = _db.Packages.Find(packageID);

        //    return 
                
        //}

    }
}
