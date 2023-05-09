using Microsoft.AspNetCore.Mvc;

namespace Paczki.Controllers
{
    public class DeliveryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
