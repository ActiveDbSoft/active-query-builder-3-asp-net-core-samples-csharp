using System.Configuration;
using System.IO;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Web.Core;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Services;
using ASP.NET_Core.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace MVC_Samples.Controllers
{
    public class ChangeConnectionController : Controller
    {
        private readonly string instanceId = "ChangeConnection";

        private readonly IHostingEnvironment _env;
        private readonly IQueryBuilderService _aqbs;
        private readonly IConfiguration _config;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public ChangeConnectionController(IQueryBuilderService aqbs, IHostingEnvironment env, IConfiguration config)
        {
            _aqbs = aqbs;
            _env = env;
            _config = config;
        }

        public ActionResult Index()
        {
            // Get an instance of the QueryBuilder object
            var qb = _aqbs.Get(instanceId);

            if (qb == null)
                qb = CreateQueryBuilder();

            return View(qb);
        }

        public ActionResult WithPartiaView()
        {
            // Get an instance of the QueryBuilder object
            var qb = _aqbs.Get(instanceId);

            if (qb == null)
                qb = CreateQueryBuilder();

            return View(qb);
        }

        private QueryBuilder CreateQueryBuilder()
        {
            // Create an instance of the QueryBuilder object
            var qb = _aqbs.Create(instanceId);

            SetNorthwindXml(qb);

            return qb;
        }

        [HttpPost]
        public ActionResult Change(string name)
        {
            ChangeConnection(name);
            return new EmptyResult();
        }

        [HttpPost]
        public PartialViewResult ChangePartial(string name)
        {
            var qb = ChangeConnection(name);
            return PartialView("_queryBuilder", qb);
        }

        public QueryBuilder ChangeConnection(string name)
        {
            var queryBuilder = _aqbs.Get(instanceId);

            queryBuilder.MetadataContainer.Clear();

            if (name == "NorthwindXmlMetaData")
                SetNorthwindXml(queryBuilder);
            else if (name == "SQLite")
                SetSqLite(queryBuilder);
            else
                SetDb2Xml(queryBuilder);

            return queryBuilder;
        }

        private void SetNorthwindXml(QueryBuilder qb)
        {
            qb.MetadataLoadingOptions.OfflineMode = true;
            qb.SyntaxProvider = new MSSQLSyntaxProvider();

            // Load MetaData from XML document. File name stored in WEB.CONFIG file in [/configuration/appSettings/Db2XmlMetaData] key
            var path = _config["NorthwindXmlMetaData"];
            var xml = Path.Combine(_env.WebRootPath, path);

            qb.MetadataContainer.ImportFromXML(xml);
            qb.MetadataStructure.Refresh();
        }

        private void SetDb2Xml(QueryBuilder qb)
        {
            qb.MetadataLoadingOptions.OfflineMode = true;
            qb.SyntaxProvider = new DB2SyntaxProvider();

            // Load MetaData from XML document. File name stored in WEB.CONFIG file in [/configuration/appSettings/Db2XmlMetaData] key
            var path = _config["Db2XmlMetaData"];
            var xml = Path.Combine(_env.WebRootPath, path);

            qb.MetadataContainer.ImportFromXML(xml);
            qb.MetadataStructure.Refresh();
        }

        private void SetSqLite(QueryBuilder qb)
        {
            qb.MetadataLoadingOptions.OfflineMode = false;
            qb.SyntaxProvider = new SQLiteSyntaxProvider();
            qb.MetadataProvider = new SQLiteMetadataProvider
            {
                Connection = DataBaseHelper.CreateSqLiteConnection(Path.Combine(_env.WebRootPath, _config["SqLiteDataBase"]))
            };

            qb.MetadataStructure.Refresh();
        }
    }
}