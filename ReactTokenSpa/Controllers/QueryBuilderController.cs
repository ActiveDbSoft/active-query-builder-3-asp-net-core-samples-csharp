using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Web.Server.Services;
using Microsoft.AspNetCore.Mvc;
using ReactTokenSpa.Providers;

namespace ReactTokenSpa.Controllers
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
    }
}