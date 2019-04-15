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
    public class MetadataStructureController : Controller
    {
        private string instanceId = "MetadataStructure";

        private readonly IHostingEnvironment _env;
        private readonly IQueryBuilderService _aqbs;
        private readonly IConfiguration _config;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public MetadataStructureController(IQueryBuilderService aqbs, IHostingEnvironment env, IConfiguration config)
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

        private QueryBuilder CreateQueryBuilder()
        {
            // Get instance of QueryBuilder
            var queryBuilder = _aqbs.Create(instanceId);
            queryBuilder.SyntaxProvider = new MSSQLSyntaxProvider();

            // Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
            queryBuilder.BehaviorOptions.AllowSleepMode = true;
            
            queryBuilder.MetadataLoadingOptions.OfflineMode = true;

            // Load MetaData from XML document.
            var path = _config["NorthwindXmlMetaData"];
            var xml = Path.Combine(_env.WebRootPath, path);

            queryBuilder.MetadataContainer.ImportFromXML(xml);

            // Initialization of the Metadata Structure object that's
            // responsible for representation of metadata in a tree-like form
            // Disable the automatic metadata structure creation
            queryBuilder.MetadataStructure.AllowChildAutoItems = false;

            // queryBuilder.DatabaseSchemaTreeOptions.DefaultExpandLevel = 0;

            MetadataFilterItem filter;

            // Create a top-level folder containing all objects
            MetadataStructureItem allObjects = new MetadataStructureItem();
            allObjects.Caption = "All objects";
            filter = allObjects.MetadataFilter.Add();
            filter.ObjectTypes = MetadataType.All;
            queryBuilder.MetadataStructure.Items.Add(allObjects);

            // Create "Favorites" folder
            MetadataStructureItem favorites = new MetadataStructureItem();
            favorites.Caption = "Favorites";
            queryBuilder.MetadataStructure.Items.Add(favorites);

            MetadataItem metadataItem;
            MetadataStructureItem item;

            // Add some metadata objects to "Favorites" folder
            metadataItem = queryBuilder.MetadataContainer.FindItem<MetadataItem>("Orders");
            item = new MetadataStructureItem();
            item.MetadataItem = metadataItem;
            favorites.Items.Add(item);

            metadataItem = queryBuilder.MetadataContainer.FindItem<MetadataItem>("Order Details");
            item = new MetadataStructureItem();
            item.MetadataItem = metadataItem;
            favorites.Items.Add(item);

            // Create folder with filter
            MetadataStructureItem filteredFolder = new MetadataStructureItem(); // creates dynamic node
            filteredFolder.Caption = "Filtered by 'Prod%'";
            filter = filteredFolder.MetadataFilter.Add();
            filter.ObjectTypes = MetadataType.Table | MetadataType.View;
            filter.Object = "Prod%";
            queryBuilder.MetadataStructure.Items.Add(filteredFolder);

            queryBuilder.SQL = GetDefaultSql();

            return queryBuilder;
        }

        private string GetDefaultSql()
        {
            return @"SELECT Orders.OrderID, Orders.CustomerID, Orders.OrderDate, [Order Details].ProductID,
					[Order Details].UnitPrice, [Order Details].Quantity, [Order Details].Discount
					FROM Orders INNER JOIN [Order Details] ON Orders.OrderID = [Order Details].OrderID
					WHERE Orders.OrderID > 0 AND [Order Details].Discount > 0";
        }
    }
}