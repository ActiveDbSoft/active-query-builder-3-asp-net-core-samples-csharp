using System.Configuration;
using System.IO;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;

namespace AspNetCoreJavaScript.Controllers
{
    public class AngularJSClientRenderingController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}