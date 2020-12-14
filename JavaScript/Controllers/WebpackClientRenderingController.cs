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
    public class WebpackClientRenderingController : Controller
    {
        private string instanceId = "Webpack";

        private readonly IHostingEnvironment _env;
        private readonly IQueryBuilderService _aqbs;
        private readonly IConfiguration _config;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public WebpackClientRenderingController(IQueryBuilderService aqbs, IHostingEnvironment env, IConfiguration config)
        {
            _aqbs = aqbs;
            _env = env;
            _config = config;
        }

        public ActionResult Index()
        {
			//Please follow the steps described in the Scripts/Webpack/README.md file to run this demo project
            CreateQueryBuilder();
            return View();
        }

        /// <summary>
        /// Creates and initializes new instance of the QueryBuilder object if it doesn't exist. 
        /// </summary>
        public void CreateQueryBuilder()
        {
            // Get an instance of the QueryBuilder object
            _aqbs.GetOrCreate(instanceId, qb => {
                qb.SyntaxProvider = new MSSQLSyntaxProvider();

                // Denies metadata loading requests from the metadata provider
                qb.MetadataLoadingOptions.OfflineMode = true;

                // Load MetaData from XML document.
                var path = _config["NorthwindXmlMetaData"];
                var xml = Path.Combine(_env.WebRootPath, path);

                qb.MetadataContainer.ImportFromXML(xml);
                qb.SQL = GetDefaultSql();
            });
        }

        private string GetDefaultSql()
        {
            return @"Select o.OrderID,
                    c.CustomerID,
                    s.ShipperID,
                    o.ShipCity
                From Orders o
                    Inner Join Customers c On o.CustomerID = c.CustomerID
                    Inner Join Shippers s On s.ShipperID = o.OrderID
                Where o.ShipCity = 'A'";
        }
    }
}