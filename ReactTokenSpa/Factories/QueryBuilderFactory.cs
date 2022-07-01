using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Infrastructure.Factories;
using ActiveQueryBuilder.Web.Server.Services;

namespace ReactTokenSpa.Factories
{
    public class QueryBuilderFactory : IQueryBuilderInstanceFactory
    {
        private readonly IQueryBuilderService _qbService;

        public QueryBuilderFactory(IQueryBuilderService qbService)
        {
            _qbService = qbService;
        }

        public QueryBuilder Create(string name)
        {
            var qb = _qbService.Create(name);
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

            return qb;
        }
    }
}
