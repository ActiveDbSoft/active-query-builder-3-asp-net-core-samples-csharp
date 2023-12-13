using Microsoft.AspNetCore.Mvc;

namespace Signalr.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}