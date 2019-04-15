using System;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AspNetCoreJavaScript.Controllers
{
    public class CreateFromConfigController : Controller
    {
        private readonly IQueryBuilderService _aqbs;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public CreateFromConfigController(IQueryBuilderService aqbs)
        {
            _aqbs = aqbs;
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
        public ActionResult Create(string name)
        {
            // Get an instance of the QueryBuilder object
            var qb = _aqbs.Get(name);

            if (qb != null)
                return new EmptyResult();

            try
            {
                // Create an instance of the QueryBuilder object
                qb = _aqbs.Create(name);

                // The necessary initialization procedures to setup SQL syntax and the source of metadata will be performed automatically 
                // according to directives in the special configuration section of 'Web.config' file.

                // This behavior is enabled by the QueryBuilderStore.WebConfig() method call in the Application_Start method in Global.asax.cs file.
                // See qb.ConfiguredBy to get information about actual default settings

                // Set default query
                qb.SQL = GetDefaultSql();
            }
            catch (Exception e)
            {
                return new StatusCodeResult(500);
            }

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