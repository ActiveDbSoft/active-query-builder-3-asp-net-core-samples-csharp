using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Web.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace ReactSpa.Controllers
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
                qb = _aqbs.Create(name);
                qb.MetadataLoadingOptions.OfflineMode = true;
                qb.SyntaxProvider = new GenericSyntaxProvider();

                // Initialize metadata
                var database = qb.MetadataContainer.AddSchema("dbo");
                var orders = database.AddTable("Orders");
                orders.AddField("Id");
                orders.AddField("Name");

                var customers = database.AddTable("Customers");
                customers.AddField("Id");
                customers.AddField("Name");

                qb.MetadataStructure.Refresh();

                return StatusCode(200);
            }
            catch (QueryBuilderException e)
            {
                return StatusCode(400, e.Message);
            }
        }
    }
}