using System;
using System.Configuration;
using System.IO;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Web.Core;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace MVC_Samples.Controllers
{
    public class SaveAndLoadUserQueriesController : Controller
    {
        private const string filename = "UserQueriesStructure.xml";

        private string instanceId = "SaveAndLoadUserQueries";

        private readonly IHostingEnvironment _env;
        private readonly IQueryBuilderService _aqbs;
        private readonly IConfiguration _config;

        // Use IActiveQueryBuilderServiceBase to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public SaveAndLoadUserQueriesController(IQueryBuilderService aqbs, IHostingEnvironment env, IConfiguration config)
        {
            _aqbs = aqbs;
            _env = env;
            _config = config;
        }

        //CUT:STD{{        
        public ActionResult Index()
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
            var queryBuilder = _aqbs.Create(instanceId);
            queryBuilder.SyntaxProvider = new MSSQLSyntaxProvider();

            // Denies metadata loading requests from the metadata provider
            queryBuilder.MetadataLoadingOptions.OfflineMode = true;

            // Load MetaData from XML document.
            var path = _config["NorthwindXmlMetaData"];
            var xml = Path.Combine(_env.WebRootPath, path);

            queryBuilder.MetadataContainer.ImportFromXML(xml);

            //Comment these 2 lines for using browser localStorage
            ImportUserQueriesFromFile(queryBuilder.UserQueries);
            queryBuilder.UserQueries.Changed += ExportUserQueriesToFile;

            //Set default query
            queryBuilder.SQL = GetDefaultSql();

            return queryBuilder;
        }

        private void ExportUserQueriesToFile(object sender, MetadataStructureItem item)
        {
            var uq = (UserQueriesContainer)sender;
            uq.ExportToXML(_env.WebRootPath + @"/../" + filename);
        }
        
        private void ImportUserQueriesFromFile(UserQueriesContainer uqc)
        {
            var file = _env.WebRootPath + @"/../" + filename;

            if (System.IO.File.Exists(file))
                uqc.ImportFromXML(file);
        }

        public void GetUserQueriesXml()
        {
            var qb = _aqbs.Get(instanceId);
            qb.UserQueries.ExportToXML(Response.Body);
        }
        
        public void LoadUserQueries(string xml)
        {
            var qb = _aqbs.Get(instanceId);
            qb.UserQueries.XML = xml;
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

//}}CUT:STD
    }
}