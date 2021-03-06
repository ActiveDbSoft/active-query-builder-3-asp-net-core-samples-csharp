﻿using System.IO;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Web.Core;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace MVC_Samples.Controllers
{
    public class NoDesignAreaController: Controller
    {
        private string instanceId = "NoDesignArea";

        private readonly IHostingEnvironment _env;
        private readonly IQueryBuilderService _aqbs;
        private readonly IConfiguration _config;

        // Use IActiveQueryBuilderServiceBase to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public NoDesignAreaController(IQueryBuilderService aqbs, IHostingEnvironment env, IConfiguration config)
        {
            _aqbs = aqbs;
            _env = env;
            _config = config;
        }

        public ActionResult Index()
        {
            // Get an instance of the QueryBuilder object
            var qb = _aqbs.GetOrCreate(instanceId, InitializeQueryBuilder);

            return View(qb);
        }

        private void InitializeQueryBuilder(QueryBuilder queryBuilder)
        {
            queryBuilder.SyntaxProvider = new MSSQLSyntaxProvider();

            // Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
            queryBuilder.BehaviorOptions.AllowSleepMode = true;
            
            // Denies metadata loading requests from the metadata provider
            queryBuilder.MetadataLoadingOptions.OfflineMode = true;

            queryBuilder.BehaviorOptions.DeleteUnusedObjects = true;
            queryBuilder.BehaviorOptions.AddLinkedObjects = true;

            // Load MetaData from XML document.
            var path = _config["NorthwindXmlMetaData"];
            var xml = Path.Combine(_env.WebRootPath, path);

            queryBuilder.MetadataContainer.ImportFromXML(xml);

            queryBuilder.SQL = GetDefaultSql();
        }

        private string GetDefaultSql()
        {
            return @"Select o.OrderID, c.CustomerID, s.ShipperID, o.ShipCity
                        From Orders o Inner Join
                          Customers c On o.Customer_ID = c.ID Inner Join
                          Shippers s On s.ID = o.Shipper_ID
                        Where o.ShipCity = 'A'";
        }
    }
}