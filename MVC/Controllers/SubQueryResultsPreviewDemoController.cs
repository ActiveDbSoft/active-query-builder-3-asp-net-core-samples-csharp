using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Core.QueryTransformer;
using ActiveQueryBuilder.Web.Core;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Services;
using ASP.NET_Core.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MVC_Samples.Controllers
{
    public class SubQueryResultsPreviewDemoController : Controller
    {
        private string instanceId = "SubQueryResultsPreview";

        private readonly IHostingEnvironment _env;
        private readonly IQueryBuilderService _aqbs;
        private readonly IConfiguration _config;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public SubQueryResultsPreviewDemoController(IQueryBuilderService aqbs, IHostingEnvironment env, IConfiguration config)
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

        public ActionResult GetData()
        {
            var qb = _aqbs.Get(instanceId);
            var conn = qb.MetadataProvider.Connection;

            var sqlQuery = new SQLQuery(qb.SQLContext)
            {
                SQL = qb.ActiveUnionSubQuery.SQL
            };

            QueryTransformer qt = new QueryTransformer
            {
                QueryProvider = sqlQuery
            };

            qt.Take("7");

            var columns = qt.Columns.Select(c => c.ResultName).ToList();

            try
            {
                var data = DataBaseHelper.GetDataList(conn, qt.SQL);
                var result = new { columns, data };
                return Json(result);
            }
            catch (Exception e)
            {
                var result = new { columns, data = new List<List<object>> { new List<object> { e.Message } } };
                return Json(result);
            }
        }

        /// <summary>
        /// Creates and initializes a new instance of the QueryBuilder object.
        /// </summary>
        /// <returns>Returns instance of the QueryBuilder object.</returns>
        private QueryBuilder CreateQueryBuilder()
        {
            // Create an instance of the QueryBuilder object
            var queryBuilder = _aqbs.Create(instanceId);
            queryBuilder.SyntaxProvider = new SQLiteSyntaxProvider();

            // Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
            queryBuilder.BehaviorOptions.AllowSleepMode = false;
            
            // Bind Active Query Builder to a live database connection.
            queryBuilder.MetadataProvider = new SQLiteMetadataProvider
            {
                // Assign an instance of DBConnection object to the Connection property.
                Connection = DataBaseHelper.CreateSqLiteConnection(Path.Combine(_env.WebRootPath, _config["SqLiteDataBase"]))
            };

            // Assign the initial SQL query text the user sees on the _first_ page load
            queryBuilder.SQL = GetDefaultSql();

            return queryBuilder;
        }

        private string GetDefaultSql()
        {
            return @"Select Count(Query1.EmployeeId) As Count_L,
                      Count(Query2.EmployeeId) As Count_C
                    From (Select employees.EmployeeId,
                            employees.LastName,
                            employees.FirstName,
                            employees.City
                          From employees
                          Where employees.City = 'Lethbridge') Query1,
                      (Select employees.EmployeeId,
                            employees.LastName,
                            employees.FirstName,
                            employees.City
                          From employees
                          Where employees.City = 'Calgary') Query2";
        }
    }
}