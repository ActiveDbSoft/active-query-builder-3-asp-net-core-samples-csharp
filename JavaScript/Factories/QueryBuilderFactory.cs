using System;
using System.Collections.Generic;
using System.IO;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Core.QueryTransformer;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Infrastructure.Factories;
using ActiveQueryBuilder.Web.Server.Services;
using AspNetCoreJavaScript.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite.Internal.UrlMatches;
using Microsoft.Extensions.Configuration;

namespace AspNetCoreJavaScript.Factories
{
    public class QueryBuilderFactory : IQueryBuilderInstanceFactory, IQueryTransformerInstanceFactory
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfiguration _config;
        private readonly IQueryBuilderService _qbService;
        private readonly IQueryTransformerService _qtService;

        private Dictionary<string, Func<string, QueryBuilder>> _qbFactories;
        private Dictionary<string, Func<string, QueryTransformer>> _qtFactories;

        public QueryBuilderFactory(IHostingEnvironment env, IConfiguration config, 
            IQueryBuilderService qbService, IQueryTransformerService qtService)
        {
            _env = env;
            _config = config;
            _qbService = qbService;
            _qtService = qtService;
        }

        public QueryBuilder Create(string name)
        {
            return GetQbFactory(name)(name);
        }

        QueryTransformer IQueryTransformerInstanceFactory.Create(string name)
        {
            return GetQtFactory(name)(name);
        }

        private Func<string, QueryBuilder> GetQbFactory(string name)
        {
            if (_qbFactories == null)
                _qbFactories = CreateQbFactories();

            _qbFactories = CreateQbFactories();
            return _qbFactories[name];
        }

        private Func<string, QueryTransformer> GetQtFactory(string name)
        {
            if (_qtFactories == null)
                _qtFactories = CreateQtFactories();

            _qtFactories = CreateQtFactories();
            return _qtFactories[name];
        }

        private Dictionary<string, Func<string, QueryBuilder>> CreateQbFactories()
        {
            return new Dictionary<string, Func<string, QueryBuilder>>
            {
                { "Angular", SimpleDemo },
                { "AngularJS", SimpleDemo },
                { "FirstClient", DoubleClientFirst },
                { "FactoryInitialization", SimpleDemo },
                { "SimpleClient", SimpleDemo },
                { "SecondClient", DoubleClientSecond },
                { "Mobile", SimpleDemo },
                { "QueryResults", QueryResults },
                { "React", SimpleDemo },
                { "Webpack", SimpleDemo },
            };
        }

        private Dictionary<string, Func<string, QueryTransformer>> CreateQtFactories()
        {
            return new Dictionary<string, Func<string, QueryTransformer>>
            {
                { "QueryResults", QueryResultsQueryTransformer }
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

        private QueryBuilder DoubleClientFirst(string name)
        {
            var qb = _qbService.Create(name);
            qb.SyntaxProvider = new MSSQLSyntaxProvider();

            // Denies metadata loading requests from the metadata provider
            qb.MetadataLoadingOptions.OfflineMode = true;

            // Load MetaData from XML document.
            var path = _config["NorthwindXmlMetaData"];
            var xml = Path.Combine(_env.WebRootPath, path);

            qb.MetadataContainer.ImportFromXML(xml);

            return qb;
        }

        private QueryBuilder DoubleClientSecond(string name)
        {
            var qb = _qbService.Create(name);
            qb.SyntaxProvider = new MSSQLSyntaxProvider();

            // Denies metadata loading requests from the metadata provider
            qb.MetadataLoadingOptions.OfflineMode = true;

            // Load MetaData from XML document.
            var path = _config["Db2XmlMetaData"];
            var xml = Path.Combine(_env.WebRootPath, path);

            qb.MetadataContainer.ImportFromXML(xml);

            return qb;
        }

        private QueryBuilder QueryResults(string name)
        {
            var qb = _qbService.Create(name);
            // Create an instance of the proper syntax provider for your database server.
            qb.SyntaxProvider = new SQLiteSyntaxProvider();

            // Turn this property on to suppress parsing error messages when user types non-SELECT statements in the text editor.
            qb.BehaviorOptions.AllowSleepMode = false;

            // Bind Active Query Builder to a live database connection.
            qb.MetadataProvider = new SQLiteMetadataProvider
            {
                // Assign an instance of DBConnection object to the Connection property.
                Connection = DataBaseHelper.CreateSqLiteConnection(Path.Combine(_env.WebRootPath, _config["SqLiteDataBase"]))
            };

            // Assign the initial SQL query text the user sees on the _first_ page load
            qb.SQL = @"Select customers.CustomerId,
                      customers.LastName,
                      customers.FirstName
                    From customers";

            return qb;
        }

        private QueryTransformer QueryResultsQueryTransformer(string name)
        {
            var qb = _qbService.Get(name) ?? QueryResults(name);
            var qt = _qtService.Create(name);
            qt.QueryProvider = qb.SQLQuery;
            qt.AlwaysExpandColumnsInQuery = true;

            return qt;
        }
    }
}
