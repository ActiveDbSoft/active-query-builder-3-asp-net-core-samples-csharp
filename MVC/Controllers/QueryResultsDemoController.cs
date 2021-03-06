﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Core.QueryTransformer;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Services;
using ASP.NET_Core.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace ASP.NET_Core.Controllers
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
            var qb = _aqbs.GetOrCreate(instanceId, InitializeQueryBuilder);
            var qt = _qts.GetOrCreate(instanceId, t =>
            {
                t.QueryProvider = qb.SQLQuery;
                t.AlwaysExpandColumnsInQuery = true;
            });

            ViewBag.QueryTransformer = qt;

            return View(qb);
        }

        [HttpPost]
        public ActionResult GetData([FromBody] GridModel m)
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
                        qt.OrderBy(c, m.Sortorder.ToLowerInvariant() == "asc");
                }
            }

            return GetData(qt, m.Params);
        }

        private ActionResult GetData(QueryTransformer qt, Param[] _params)
        {
            try
            {
                var data = Execute(qt, _params);
                return Json(data);
            }
            catch (Exception e)
            {
                return Json(new ErrorOutput { Error = e.Message });
            }
        }

        private List<Dictionary<string, object>> Execute(QueryTransformer qt, Param[] _params)
        {
            var conn = qt.Query.SQLContext.MetadataProvider.Connection;
            var sql = qt.SQL;

            if (_params != null)
                foreach (var p in _params)
                    p.DataType = qt.Query.QueryParameters.First(qp => qp.FullName == p.Name).DataType;

            return DataBaseHelper.GetData(conn, sql, _params);
        }

        [HttpPost]
        public ActionResult SelectRecordsCount(Param[] _params)
        {
            var qb = _aqbs.Get(instanceId);
            var qt = _qts.Get(instanceId);
            var qtForSelectRecordsCount = new QueryTransformer { QueryProvider = qb.SQLQuery };

            try
            {
                qtForSelectRecordsCount.Assign(qt);
                qtForSelectRecordsCount.Skip("");
                qtForSelectRecordsCount.Take("");
                qtForSelectRecordsCount.SelectRecordsCount("recCount");

                try
                {
                    var data = Execute(qtForSelectRecordsCount, _params);
                    return Json(data.First().Values.First());
                }
                catch (Exception e)
                {
                    return Json(new ErrorOutput {Error = e.Message});
                }
            }
            finally
            {
                qtForSelectRecordsCount.Dispose();
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
        private void InitializeQueryBuilder(QueryBuilder queryBuilder)
        {
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