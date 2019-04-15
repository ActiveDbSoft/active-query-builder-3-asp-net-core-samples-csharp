using System.IO;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Web.Core;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace ASP.NET_Core.Controllers
{
    public class HtmlHelpersDemoController : Controller
    {
        private readonly IQueryBuilderService _aqbs;
        private readonly IHostingEnvironment _env;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public HtmlHelpersDemoController(IQueryBuilderService aqbs, IHostingEnvironment env)
        {
            _aqbs = aqbs;
            _env = env;
        }

        // GET
        public IActionResult Index()
        {
            var qb = _aqbs.Get("HtmlHelpers");

            if (qb == null)
                qb = CreateQueryBuilder();

            return View(qb);
        }

        private QueryBuilder CreateQueryBuilder()
        {
            // Create an instance of the QueryBuilder object.
            var queryBuilder = _aqbs.Create("HtmlHelpers");

            // Create an instance of the proper syntax provider for your database server.
            queryBuilder.SyntaxProvider = new MSSQLSyntaxProvider();

            // Denies metadata loading requests from live database connection.
            queryBuilder.MetadataLoadingOptions.OfflineMode = true;

            // Load MetaData from the pre-generated XML document.
            var path = "../../Sample databases/Northwind.xml";
            var xml = Path.Combine(_env.WebRootPath, path);

            queryBuilder.MetadataContainer.ImportFromXML(xml);

            //Set default query
            queryBuilder.SQL = GetDefaultSql();

            return queryBuilder;
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