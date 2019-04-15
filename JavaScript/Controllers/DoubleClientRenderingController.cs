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
    public class DoubleClientRenderingController : Controller
    {
        private readonly IHostingEnvironment _env;
        private readonly IQueryBuilderService _aqbs;
        private readonly IConfiguration _config;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public DoubleClientRenderingController(IQueryBuilderService aqbs, IHostingEnvironment env, IConfiguration config)
        {
            _aqbs = aqbs;
            _env = env;
            _config = config;
        }

        public ActionResult Index()
        {
            CreateFirstQueryBuilder();
            CreateSecondQueryBuilder();

            return View();
        }

        /// <summary>
        /// Creates and initializes the first instance of the QueryBuilder object if it doesn't exist. 
        /// </summary>
        private void CreateFirstQueryBuilder()
        {
            // Get an instance of the QueryBuilder object
            var queryBuilder = _aqbs.Get("FirstClient");

            if (queryBuilder != null)
                return;

            // Create an instance of the QueryBuilder object
            queryBuilder = _aqbs.Create("FirstClient");
            queryBuilder.SyntaxProvider = new MSSQLSyntaxProvider();
            
            // Denies metadata loading requests from the metadata provider
            queryBuilder.MetadataLoadingOptions.OfflineMode = true;

            // Load MetaData from XML document.
            var path = _config["NorthwindXmlMetaData"];
            var xml = Path.Combine(_env.WebRootPath, path);

            queryBuilder.MetadataContainer.ImportFromXML(xml);
        }

        /// <summary>
        /// Creates and initializes the second instance of the QueryBuilder object if it doesn't exist. 
        /// </summary>
        private void CreateSecondQueryBuilder()
        {
            // Get an instance of the QueryBuilder object
            var queryBuilder = _aqbs.Get("SecondClient");

            if (queryBuilder != null)
                return;

            // Create an instance of the QueryBuilder object
            queryBuilder = _aqbs.Create("SecondClient");
            queryBuilder.SyntaxProvider = new DB2SyntaxProvider();
            
            // Denies metadata loading requests from the metadata provider
            queryBuilder.MetadataLoadingOptions.OfflineMode = true;

            // Load MetaData from XML document.
            var path = _config["Db2XmlMetaData"];
            var xml = Path.Combine(_env.WebRootPath, path);

            queryBuilder.MetadataContainer.ImportFromXML(xml);
        }
    }
}