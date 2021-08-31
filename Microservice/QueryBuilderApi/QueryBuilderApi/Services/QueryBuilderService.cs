using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ActiveQueryBuilder.Core.QueryTransformer;
using ActiveQueryBuilder.Web.Server.Services;
using ActiveQueryBuilder.Web.Server;
using QueryBuilderApi.Dtos;

namespace QueryBuilderApi.Services
{
    public interface IQueryBuilderService
    {
        (QueryBuilder, QueryTransformer) GetOrCreate(string token);
        string GetQuery(string token, int? start, int? end, List<SortingModel> sorting);
        string GetQueryCount(string token);
    }

    public class QueryBuilderService : IQueryBuilderService
    {
        private readonly ActiveQueryBuilder.Web.Server.Services.IQueryBuilderService _queryBuilderService;
        private readonly IQueryTransformerService _queryTransformerService;
        private readonly QueryBuilderMetadataStorage _metadataStorage;
        private static readonly object LockObject = new object();

        public QueryBuilderService(ActiveQueryBuilder.Web.Server.Services.IQueryBuilderService queryBuilderService,
            IQueryTransformerService queryTransformerService, QueryBuilderMetadataStorage metadataStorage)
        {
            _queryBuilderService = queryBuilderService;
            _queryTransformerService = queryTransformerService;
            _metadataStorage = metadataStorage;
        }

        public (QueryBuilder, QueryTransformer) GetOrCreate(string token)
        {
            try
            {
                lock (LockObject)
                {
                    var queryBuilder = _queryBuilderService.GetOrCreate(token, qb =>
                    {
                        qb.MetadataLoadingOptions.OfflineMode = true;
                        _metadataStorage.SetMetadata(qb);
                    });

                    var queryTransformer = _queryTransformerService.GetOrCreate(token, qt => qt.QueryProvider = queryBuilder.SQLQuery);
                    queryBuilder.MetadataStructure.Refresh();

                    return (queryBuilder, queryTransformer);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public string GetQueryCount(string token)
        {
            lock (LockObject)
            {
                var (_, qt) = GetOrCreate(token);

                qt.ResultCount = null;
                qt.ResultOffset = null;
                qt.SelectRecordsCount();

                var sqlCount = qt.SQL;

                qt.Aggregations.Clear();

                return sqlCount;
            }
        }

        public string GetQuery(string token, int? start, int? end, List<SortingModel> sorting)
        {
            lock (LockObject)
            {
                var (_, qt) = GetOrCreate(token);

                if (start.HasValue && end.HasValue)
                    qt.Skip(start.ToString()).Take((end - start).ToString());

                if (sorting == null)
                    return qt.SQL;

                qt.Sortings.Clear();

                foreach (var sortingModel in sorting)
                {
                    if (string.IsNullOrEmpty(sortingModel.Sort))
                        continue;

                    var column = qt.Columns.FindColumnByResultName(sortingModel.Field);
                    if (column == null)
                        continue;

                    qt.Sortings.Add(sortingModel.Sort.ToLower() == "asc" ? column.Asc() : column.Desc());
                }

                return qt.SQL;
            }
        }
    }
    public class SortingModel
    {
        public string Field { get; set; }
        public string Sort { get; set; }
    }
}
