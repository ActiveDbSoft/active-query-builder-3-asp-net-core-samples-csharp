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
            try
            {
                // Create an instance of the QueryBuilder object
                _aqbs.GetOrCreate(name, q => q.SQL = GetDefaultSql());

                // The necessary initialization procedures to setup SQL syntax and the source of metadata will be performed automatically 
                // according to directives in the special configuration section.

                // This behavior is enabled by the AddJsonFile or AddXmlFile methods call in the Startup method in Startup.cs file.
                // See qb.ConfiguredBy to get information about actual default settings
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
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