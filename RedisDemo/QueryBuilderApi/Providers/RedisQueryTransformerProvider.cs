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
            return ExecuteWithLock(id, () => GetByCacheKey(GetCacheKey(id), id));
        }

        private QueryTransformer GetByCacheKey(string key, string id)
        {
            var qb = _queryBuilderProvider.Get(id);
            var stateXml = _cache.GetString(key);
            if (string.IsNullOrEmpty(stateXml))
                return new QueryTransformer { Tag = id, QueryProvider = qb.SQLQuery };

            _cache.Refresh(key);
            return new QueryTransformer { Tag = id, XML = stateXml, QueryProvider = qb.SQLQuery };
        }

        public void Put(QueryTransformer qt)
        {
            var instanceId = qt.Tag.ToString();
            ExecuteWithLock(instanceId, () => _cache.SetString(GetCacheKey(instanceId), qt.XML));
        }

        public void Delete(string id)
        {
            ExecuteWithLock(id, () => _cache.Remove(GetCacheKey(id)));
        }

        protected override string GetCacheKey(string id)
        {
            return base.GetCacheKey(id) + "-qt";
        }
    }
}
