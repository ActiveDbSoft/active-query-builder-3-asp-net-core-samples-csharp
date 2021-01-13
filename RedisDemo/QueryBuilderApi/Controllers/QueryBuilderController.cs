using System;
using System.Collections.Generic;
using System.Data;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Core.QueryTransformer;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Infrastructure.Providers;
using ActiveQueryBuilder.Web.Server.Services;
using Microsoft.AspNetCore.Mvc;
using QueryBuilderApi.Providers;

namespace QueryBuilderApi.Controllers
{
    public class QueryBuilderController : Controller
    {
        private readonly IQueryBuilderService _aqbs;
        private readonly RedisQueryBuilderProvider _queryBuilderProvider;
        private readonly IQueryTransformerService _qts;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public QueryBuilderController(IQueryBuilderService aqbs, RedisQueryBuilderProvider queryBuilderProvider, 
            IQueryTransformerService qts)
        {
            _aqbs = aqbs;
            _queryBuilderProvider = queryBuilderProvider;
            _qts = qts;
        }

        public string CheckToken(string token, string instanceId)
        {
            // check if the item with specified key exists in the storage. 
            if (_queryBuilderProvider.CheckToken(token, instanceId))
                // Return empty string in the case of success
                return string.Empty;

            // Return the new token to the client if the specified token doesn't exist.
            return _queryBuilderProvider.CreateToken();
        }

        /// <summary>
        /// Creates and initializes new instance of the QueryBuilder object for the given identifier if it doesn't exist. 
        /// </summary>
        /// <param name="name">Instance identifier of object in the current session.</param>
        /// <returns></returns>
        public ActionResult CreateQueryBuilder(string name)
        {
            try
            {
                // Create an instance of the QueryBuilder object
                _aqbs.Get(name);

                return StatusCode(200);
            }
            catch (QueryBuilderException e)
            {
                return StatusCode(400, e.Message);
            }
        }

        [Route("getSql"), HttpPost]
        public ActionResult GetSql([FromBody] GetSqlModel model)
        {
            var qt = _qts.Get(model.InstanceId);

            qt.Skip((model.Pagenum * model.Pagesize).ToString());
            qt.Take(model.Pagesize == 0 ? "" : model.Pagesize.ToString());

            if (!string.IsNullOrEmpty(model.Sortdatafield))
            {
                qt.Sortings.Clear();

                if (!string.IsNullOrEmpty(model.Sortorder))
                {
                    var c = qt.Columns.FindColumnByResultName(model.Sortdatafield);

                    if (c != null)
                        qt.OrderBy(c, model.Sortorder.ToLowerInvariant() == "asc");
                }
            }

            return Content(qt.SQL);
        }

        [Route("getRecordCountSql"), HttpPost]
        public ActionResult GetRecordCountSql([FromBody] GetRecordCountSqlModel model)
        {
            var qt = _qts.Get(model.InstanceId);

            using (var qtForSelectRecordsCount = new QueryTransformer { QueryProvider = qt.QueryProvider })
            {
                qtForSelectRecordsCount.Assign(qt);
                qtForSelectRecordsCount.Skip("");
                qtForSelectRecordsCount.Take("");
                qtForSelectRecordsCount.SelectRecordsCount("recCount");

                return Content(qtForSelectRecordsCount.SQL);
            }
        }
    }

    public class GetSqlModel
    {
        public int Pagenum { get; set; }
        public int Pagesize { get; set; }
        public string Sortdatafield { get; set; }
        public string Sortorder { get; set; }

        public string InstanceId { get; set; }
    }

    public class GetRecordCountSqlModel
    {
        public string InstanceId { get; set; }
    }
}
