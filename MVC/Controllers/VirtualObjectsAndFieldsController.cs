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
    public class VirtualObjectsAndFieldsController : Controller
    {
        private string instanceId = "VirtualObjectsAndFields";

        private readonly IHostingEnvironment _env;
        private readonly IQueryBuilderService _aqbs;
        private readonly IConfiguration _config;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public VirtualObjectsAndFieldsController(IQueryBuilderService aqbs, IHostingEnvironment env, IConfiguration config)
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
            // Create an instance of the QueryBuilder object
            var queryBuilder = _aqbs.Create(instanceId);
            queryBuilder.SyntaxProvider = new MSSQLSyntaxProvider();

            // Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
            queryBuilder.BehaviorOptions.AllowSleepMode = true;

            // Denies metadata loading requests from the metadata provider
            queryBuilder.MetadataLoadingOptions.OfflineMode = true;

            // Load MetaData from XML document.
            var path = _config["NorthwindXmlMetaData"];
            var xml = Path.Combine(_env.WebRootPath, path);

            queryBuilder.MetadataContainer.ImportFromXML(xml);

            MetadataObject o;
            MetadataField f;

            // Virtual fields for real object
            // ===========================================================================
            o = queryBuilder.MetadataContainer.FindItem<MetadataObject>("Orders");

            // first test field - simple expression
            f = o.AddField("OrderId_plus_1");
            f.Expression = "orders.OrderId + 1";

            // second test field - correlated sub-query
            f = o.AddField("CustomerCompanyName");
            f.Expression = "(select c.CompanyName from Customers c where c.CustomerId = orders.CustomerId)";

            // Virtual object (table) with virtual fields
            // ===========================================================================

            o = queryBuilder.MetadataContainer.AddTable("MyOrders");
            o.Expression = "Orders";

            // first test field - simple expression
            f = o.AddField("OrderId_plus_1");
            f.Expression = "MyOrders.OrderId + 1";

            // second test field - correlated sub-query
            f = o.AddField("CustomerCompanyName");
            f.Expression = "(select c.CompanyName from Customers c where c.CustomerId = MyOrders.CustomerId)";

            // Virtual object (sub-query) with virtual fields
            // ===========================================================================

            o = queryBuilder.MetadataContainer.AddTable("MyBetterOrders");
            o.Expression = "(select OrderId, CustomerId, OrderDate from Orders)";

            // first test field - simple expression
            f = o.AddField("OrderId_plus_1");
            f.Expression = "MyBetterOrders.OrderId + 1";

            // second test field - correlated sub-query
            f = o.AddField("CustomerCompanyName");
            f.Expression = "(select c.CompanyName from Customers c where c.CustomerId = MyBetterOrders.CustomerId)";

            queryBuilder.SQLQuery.SQLUpdated += OnSQLUpdated;

            queryBuilder.SQL = "SELECT mbo.OrderId_plus_1, mbo.CustomerCompanyName FROM MyBetterOrders mbo";

            return queryBuilder;
        }

        public void OnSQLUpdated(object sender, EventArgs e)
        {
            var qb = _aqbs.Get(instanceId);

            var opts = new SQLFormattingOptions();

            opts.Assign(qb.SQLFormattingOptions);
            opts.KeywordFormat = KeywordFormat.UpperCase;

            // get query with virtual objects and fields
            opts.ExpandVirtualObjects = false;
            var sqlWithVirtObjects = FormattedSQLBuilder.GetSQL(qb.SQLQuery.QueryRoot, opts);

            // get SQL query with real object names
            opts.ExpandVirtualObjects = true;
            var plainSql = FormattedSQLBuilder.GetSQL(qb.SQLQuery.QueryRoot, opts);

            // prepare additional data to be sent to the client
            qb.ExchangeData = new
            {
                SQL = plainSql,
                VirtualObjectsSQL = sqlWithVirtObjects
            };
        }
    }
}