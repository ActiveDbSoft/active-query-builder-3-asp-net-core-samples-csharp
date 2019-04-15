using System;
using ActiveQueryBuilder.Web.Core;
using ActiveQueryBuilder.Web.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ASP.NET_Core.Controllers
{
    public class CreateFromConfigDemoController : Controller
    {
        private readonly IQueryBuilderService _aqbs;

        // Use IActiveQueryBuilderServiceBase to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public CreateFromConfigDemoController(IQueryBuilderService aqbs)
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
                // according to directives in the special configuration section.

                // This behavior is enabled by the AddJsonFile or AddXmlFile methods call in the Startup method in Startup.cs file.
                // See qb.ConfiguredBy to get information about actual default settings

                // Set default query
                qb.SQL = GetDefaultSql();
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
            return "Select * From \"Departments\" Inner Join \"Employees\" On \"Departments\".\"Department ID\" = \"Employees\".\"Working Department\" Inner Join \"Employees\" Employees1 On \"Departments\".\"Department Manager\" = Employees1.\"Employee ID\"";
        }
    }
}