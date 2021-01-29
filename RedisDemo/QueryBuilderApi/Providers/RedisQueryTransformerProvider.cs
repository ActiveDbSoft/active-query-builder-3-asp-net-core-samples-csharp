using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActiveQueryBuilder.Core.QueryTransformer;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Infrastructure.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace QueryBuilderApi.Providers
{
    public class RedisQueryTransformerProvider : RedisProviderBase, IQueryTransformerProvider
    {
        private readonly IDistributedCache _cache;
        private readonly IQueryBuilderProvider _queryBuilderProvider;

        public RedisQueryTransformerProvider(IHttpContextAccessor accessor, IDistributedCache cache, 
            RedlockFactoryProvider lockFactoryProvider, IQueryBuilderProvider queryBuilderProvider)
            : base(accessor, lockFactoryProvider)
        {
            _cache = cache;
            _queryBuilderProvider = queryBuilderProvider;
        }

        public bool SaveState => true;

        public QueryTransformer Get(string id)
        {
            var key = GetCacheKey(id);
            return ExecuteWithLock(key, () => GetByCacheKey(key, id));
        }

        private QueryTransformer GetByCacheKey(string key, string id)
        {
            var qb = _queryBuilderProvider.Get(id);
            var stateXml = _cache.GetString(key);

            if (string.IsNullOrEmpty(stateXml))
                return new QueryTransformer { Tag = id, QueryProvider = qb.SQLQuery };

            _cache.Refresh(key);
            return new QueryTransformer { QueryProvider = qb.SQLQuery, Tag = id, XML = stateXml };
        }

        public void Put(QueryTransformer qt)
        {
            var key = GetCacheKey(qt.Tag.ToString());
            ExecuteWithLock(key, () => _cache.SetString(key, qt.XML));
        }

        public void Delete(string id)
        {
            var key = GetCacheKey(id);
            ExecuteWithLock(key, () => _cache.Remove(key));
        }

        protected override string GetCacheKey(string id)
        {
            return base.GetCacheKey(id) + "-qt";
        }
    }
}
