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
    public class ChangeConnectionController: Controller
    {
        private string instanceId = "ChangeConnection";

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

            if (_aqbs.Get(instanceId) == null)
                _aqbs.Create(instanceId);
        }

        public ActionResult Index()
        {
            return View();
        }

        public void Change(string name)
        {
            var queryBuilder = _aqbs.Get(instanceId);

            queryBuilder.SQLQuery.Clear();
            queryBuilder.MetadataContainer.Clear();

            if (name == "NorthwindXmlMetaData")
                SetNorthwindXml(queryBuilder);
            else 
                SetDb2Xml(queryBuilder);
        }

        private void SetNorthwindXml(QueryBuilder qb)
        {
            qb.MetadataLoadingOptions.OfflineMode = true;
            qb.SyntaxProvider = new MSSQLSyntaxProvider();

            // Load MetaData from XML document.
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
    }
}