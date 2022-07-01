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
    public class AngularClientRenderingController : Controller
    {
        private readonly IHostingEnvironment _env;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public AngularClientRenderingController(IHostingEnvironment env)
        {
            _env = env;
        }

        public ActionResult Index()
        {
			//Please follow the steps described in the wwwroot/js/Angular/README.md file to run this demo project
            try
            {
                var file = System.IO.File.OpenRead(Path.Combine(_env.WebRootPath, "js/Angular/dist/index.html"));
                return File(file, "text/html");
            }
            catch (IOException)
            {
                return new StatusCodeResult(400);
            }
        }
    }
}