using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Web.Server.Infrastructure.Providers;
using ActiveQueryBuilder.Web.Server.Services;
using Microsoft.AspNetCore.Mvc;
using RedisStorage.Providers;

namespace RedisStorage.Controllers
{
    public class QueryBuilderController : Controller
    {
        private readonly IQueryBuilderService _aqbs;
        private readonly RedisQueryBuilderProvider _queryBuilderProvider;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public QueryBuilderController(IQueryBuilderService aqbs, RedisQueryBuilderProvider queryBuilderProvider)
        {
            _aqbs = aqbs;
            _queryBuilderProvider = queryBuilderProvider;
        }

        public string CheckToken(string token)
        {
            // check if the item with specified key exists in the storage. 
            if (_queryBuilderProvider.CheckToken(token))
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
            // Get an instance of the QueryBuilder object
            var qb = _aqbs.Get(name);

            if (qb != null)
                return StatusCode(200);

            try
            {
                // Create an instance of the QueryBuilder object
                _aqbs.Create(name);

                return StatusCode(200);
            }
            catch (QueryBuilderException e)
            {
                return StatusCode(400, e.Message);
            }
        }

        [Route("getSql/{token}")]
        public ActionResult GetSql(string token)
        {
            var sql = _queryBuilderProvider.GetSql(token);
            if (string.IsNullOrEmpty(sql))
                return NotFound();

            return Content(sql);
        }
    }
}
