using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Web.Core;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Services;
using AspNetCoreCrossDomain.Providers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AspNetCoreCrossDomain.Controllers
{
    public class QueryBuilderController : Controller
    {
        private readonly IQueryBuilderService _aqbs;

        // Use IQueryBuilderService to get access to the server-side instances of Active Query Builder objects. 
        // See the registration of this service in the Startup.cs.
        public QueryBuilderController(IQueryBuilderService aqbs)
        {
            _aqbs = aqbs;
        }

        public string CheckToken(string token)
        {
            // get Token QueryBuilder provider from the store
            var provider = (TokenQueryBuilderProvider)_aqbs.Provider;

            // check if the item with specified key exists in the storage. 
            if (provider.CheckToken(token))
            	// Return empty string in the case of success
                return string.Empty;
            // Return the new token to the client if the specified token doesn't exist.
            return provider.CreateToken();
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
    }
}