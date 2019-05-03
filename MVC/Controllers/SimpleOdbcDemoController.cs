using System;
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
    public class SimpleOdbcDemoController : Controller
    {
        private string instanceId = "SimpleOledbDemo";
        
        private readonly IQueryBuilderService _aqbs;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public SimpleOdbcDemoController(IQueryBuilderService aqbs)
        {
            _aqbs = aqbs;
        }

        public ActionResult Index()
        {
            // Get an instance of the QueryBuilder object
            var qb = _aqbs.Get(instanceId);

            if (qb == null)
                qb = CreateQueryBuilder();

            return View(qb);
        }

        /// <summary>
        /// Creates and initializes a new instance of the QueryBuilder object.
        /// </summary>
        /// <returns>Returns instance of the QueryBuilder object.</returns>
        private QueryBuilder CreateQueryBuilder()
        {
            // Define connection string here
            var connectionString = "";

            if (string.IsNullOrEmpty(connectionString))
                throw new ApplicationException("Connection string is not set");

            // Create an instance of the QueryBuilder object
            var queryBuilder = _aqbs.Create(instanceId);
            queryBuilder.SyntaxProvider = new AutoSyntaxProvider();

            // Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
            queryBuilder.BehaviorOptions.AllowSleepMode = false;

            // Bind Active Query Builder to a live database connection.
            queryBuilder.MetadataProvider = new ODBCMetadataProvider
            {
                // Assign an instance of DBConnection object to the Connection property.
                Connection = DataBaseHelper.CreateOdbcConnection(connectionString)
            };

            // Assign the initial SQL query text the user sees on the _first_ page load
            queryBuilder.SQL = GetDefaultSql();

            return queryBuilder;
        }

        private string GetDefaultSql()
        {
            return @"Select o.OrderID,
                      c.CustomerID As a1,
                      c.CompanyName,
                      s.ShipperID
                    From (Orders o
                      Inner Join Customers c On o.CustomerID = c.CustomerID)
                      Inner Join Shippers s On s.ShipperID = o.ShipperID
                    Where o.Ship_City = 'A'";
        }
    }
}