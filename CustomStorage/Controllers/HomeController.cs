using System;
using ActiveQueryBuilder.Web.Server;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreCustomStorage.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}