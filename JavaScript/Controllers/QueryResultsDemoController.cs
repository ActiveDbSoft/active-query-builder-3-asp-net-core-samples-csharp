﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Core.QueryTransformer;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Services;
using AspNetCoreJavaScript.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AspNetCoreJavaScript.Controllers
{
    public class QueryResultsDemoController : Controller
    {
        private string instanceId = "QueryResults";

        private readonly IHostingEnvironment _env;
        private readonly IQueryBuilderService _aqbs;
        private readonly IQueryTransformerService _qts;
        private readonly IConfiguration _config;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public QueryResultsDemoController(IQueryBuilderService aqbs, IQueryTransformerService qts, IHostingEnvironment env, IConfiguration config)
        {
            _aqbs = aqbs;
            _qts = qts;
            _env = env;
            _config = config;
        }

        public ActionResult Index()
        {
            // Get an instance of the QueryBuilder object
            var qb = _aqbs.Get(instanceId);
            var qt = _qts.Get(instanceId);

            if (qb == null)
                qb = CreateQueryBuilder();

            if (qt == null)
                qt = CreateQueryTransformer(qb.SQLQuery);

            ViewBag.QueryTransformer = qt;

            return View(qb);
        }

        public ActionResult GetData(GridModel m)
        {
            var qt = _qts.Get(instanceId);

            qt.Skip((m.Pagenum * m.Pagesize).ToString());
            qt.Take(m.Pagesize == 0 ? "" : m.Pagesize.ToString());

            if (!string.IsNullOrEmpty(m.Sortdatafield))
            {
                qt.Sortings.Clear();

                if (!string.IsNullOrEmpty(m.Sortorder))
                {
                    var c = qt.Columns.FindColumnByResultName(m.Sortdatafield);

                    if (c != null)
                        qt.OrderBy(c, m.Sortorder.ToLower() == "asc");
                }
            }

            return GetData(qt, m.Params);
        }

        private ActionResult GetData(QueryTransformer qt, Param[] _params)
        {
            var conn = DataBaseHelper.CreateSqLiteConnection(GetDataBasePath());

            var sql = qt.SQL;

            if (_params != null)
                foreach (var p in _params)
                    p.DataType = qt.Query.QueryParameters.First(qp => qp.FullName == p.Name).DataType;

            try
            {
                var data = DataBaseHelper.GetData(conn, sql, _params);
                return Json(data);
            }
            catch (Exception e)
            {
                return Json(new ErrorOutput { Error = e.Message });
            }
        }

        private class ErrorOutput
        {
            public string Error { get; set; }
        }

        /// <summary>
        /// Creates and initializes a new instance of the QueryBuilder object.
        /// </summary>
        /// <returns>Returns instance of the QueryBuilder object.</returns>
        private QueryBuilder CreateQueryBuilder()
        {
            // Create an instance of the QueryBuilder object
            var queryBuilder = _aqbs.Create(instanceId);

            // Create an instance of the proper syntax provider for your database server.
            queryBuilder.SyntaxProvider = new SQLiteSyntaxProvider();

            // Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
            queryBuilder.BehaviorOptions.AllowSleepMode = false;

            // Bind Active Query Builder to a live database connection.
            queryBuilder.MetadataProvider = new SQLiteMetadataProvider
            {
                // Assign an instance of DBConnection object to the Connection property.
                Connection = DataBaseHelper.CreateSqLiteConnection(GetDataBasePath())
            };

            // Assign the initial SQL query text the user sees on the _first_ page load
            queryBuilder.SQL = GetDefaultSql();

            return queryBuilder;
        }

        [HttpPost]
        public ActionResult SelectRecordsCount([FromBody] Param[] _params)
        {
            var qb = _aqbs.Get(instanceId);
            var qt = _qts.Get(instanceId);
            var qtForSelectRecordsCount = _qts.Create(instanceId + "_for_records_count");

            qtForSelectRecordsCount.QueryProvider = qb.SQLQuery;
            qtForSelectRecordsCount.Assign(qt);
            qtForSelectRecordsCount.Skip("");
            qtForSelectRecordsCount.Take("");
            qtForSelectRecordsCount.SelectRecordsCount("recCount");

            var result = GetData(qtForSelectRecordsCount, _params);

            _qts.Remove(instanceId + "_for_records_count");

            return result;
        }

        private string GetDataBasePath()
        {
            var path = _config["SqLiteDataBase"];
            return Path.Combine(_env.WebRootPath, path);
        }

        private string GetDefaultSql()
        {
            return @"Select customers.CustomerId,
                      customers.LastName,
                      customers.FirstName
                    From customers";
        }

        /// <summary>
        /// Creates and initializes a new instance of the QueryTransformer object.
        /// </summary>
        /// <param name="query">SQL Query to transform.</param>
        /// <returns>Returns instance of the QueryTransformer object.</returns>
        private QueryTransformer CreateQueryTransformer(SQLQuery query)
        {
            var qt = _qts.Create(instanceId);

            qt.QueryProvider = query;
            qt.AlwaysExpandColumnsInQuery = true;

            return qt;
        }
    }

    public class GridModel
    {
        public int Pagenum { get; set; }
        public int Pagesize { get; set; }
        public string Sortdatafield { get; set; }
        public string Sortorder { get; set; }
        public Param[] Params { get; set; }
    }

    public class Param
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public DbType DataType { get; set; }
    }
}
