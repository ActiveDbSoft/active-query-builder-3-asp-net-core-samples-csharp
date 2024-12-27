using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Infrastructure.Factories;
using ActiveQueryBuilder.Web.Server.Services;

namespace Blazor.Factories
{
    public class QueryBuilderFactory : IQueryBuilderInstanceFactory
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly IQueryBuilderService _qbService;

        private Dictionary<string, Func<string, QueryBuilder>> _qbFactories;

        public QueryBuilderFactory(
            IWebHostEnvironment env,
            IConfiguration config,
            IQueryBuilderService qbService)
        {
            _env = env;
            _config = config;
            _qbService = qbService;
        }

        public QueryBuilder Create(string name)
        {
            return GetQbFactory(name)(name);
        }

        private Func<string, QueryBuilder> GetQbFactory(string name)
        {
            if (_qbFactories == null)
                _qbFactories = CreateQbFactories();

            _qbFactories = CreateQbFactories();
            return _qbFactories[name];
        }

        private Dictionary<string, Func<string, QueryBuilder>> CreateQbFactories()
        {
            return new Dictionary<string, Func<string, QueryBuilder>>
            {
                { "Blazor", SimpleDemo },
            };
        }

        private QueryBuilder SimpleDemo(string name)
        {
            var qb = _qbService.Create(name);
            qb.SyntaxProvider = new MSSQLSyntaxProvider();

            // Denies metadata loading requests from the metadata provider
            qb.MetadataLoadingOptions.OfflineMode = true;

            // Load MetaData from XML document.
            var path = _config["NorthwindXmlMetaData"];
            var xml = Path.Combine(_env.WebRootPath, path);

            qb.MetadataContainer.ImportFromXML(xml);
            qb.SQL = @"Select o.OrderID,
                    c.CustomerID,
                    s.ShipperID,
                    o.ShipCity
                From Orders o
                    Inner Join Customers c On o.CustomerID = c.CustomerID
                    Inner Join Shippers s On s.ShipperID = o.OrderID
                Where o.ShipCity = 'A'";

            return qb;
        }
    }
}
