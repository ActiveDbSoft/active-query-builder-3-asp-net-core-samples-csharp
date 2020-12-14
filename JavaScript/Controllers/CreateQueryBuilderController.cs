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
    public class CreateQueryBuilderController : Controller
    {
        private readonly IHostingEnvironment _env;
        private readonly IQueryBuilderService _aqbs;
        private readonly IConfiguration _config;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public CreateQueryBuilderController(IQueryBuilderService aqbs, IHostingEnvironment env, IConfiguration config)
        {
            _aqbs = aqbs;
            _env = env;
            _config = config;
        }

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Creates and initializes new instance of the QueryBuilder object for the given identifier if it doesn't exist. 
        /// </summary>
        /// <param name="name">Instance identifier of object in the current session.</param>
        /// <returns></returns>
        public ActionResult CreateQueryBuilder(string name)
        {
            // Get an instance of the QueryBuilder object
            _aqbs.GetOrCreate(name, qb => {
                qb.SyntaxProvider = new MSSQLSyntaxProvider();

                // Denies metadata loading requests from the metadata provider
                qb.MetadataLoadingOptions.OfflineMode = true;

                // Load MetaData from XML document.
                var path = _config["NorthwindXmlMetaData"];
                var xml = Path.Combine(_env.WebRootPath, path);

                qb.MetadataContainer.ImportFromXML(xml);

                //Set default query
                qb.SQL = GetDefaultSql();
            });

            return new EmptyResult();
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