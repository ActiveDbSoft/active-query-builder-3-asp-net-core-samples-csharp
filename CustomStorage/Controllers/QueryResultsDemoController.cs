using System;
using System.Data;
using System.Linq;
using System.Net;
using ActiveQueryBuilder.Core.QueryTransformer;
using ActiveQueryBuilder.Web.Core;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Services;
using AspNetCoreCustomStorage.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AspNetCoreCustomStorage.Controllers
{
    public class QueryResultsDemoController : Controller
    {
        private string instanceId = "QueryResults";
        
        private readonly IQueryBuilderService _aqbs;
        private readonly IQueryTransformerService _qts;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public QueryResultsDemoController(IQueryBuilderService aqbs, IQueryTransformerService qts)
        {
            _aqbs = aqbs;
            _qts = qts;
        }

        public ActionResult Index()
        {
            return View();
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
            var conn = qt.Query.SQLContext.MetadataProvider.Connection;
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
                return StatusCode((int) HttpStatusCode.BadRequest, e.Message);
            }
        }

        public void LoadQuery(string query)
        {
            var qb = _aqbs.Get(instanceId);

            if (query == "artist")
                qb.SQL = "Select artists.ArtistId, artists.Name From artists";
            else
                qb.SQL = "Select tracks.TrackId, tracks.Name From tracks";

            _aqbs.Put(qb);
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