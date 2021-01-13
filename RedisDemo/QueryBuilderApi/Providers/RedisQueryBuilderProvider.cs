using System;
using ActiveQueryBuilder.Core.QueryTransformer;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Configuration;
using ActiveQueryBuilder.Web.Server.Infrastructure.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using QueryBuilderApi.Controllers;
using RedLockNet;
using RedLockNet.SERedis;

namespace QueryBuilderApi.Providers
{
    public class RedisQueryBuilderProvider : RedisProviderBase, IQueryBuilderProvider
    {
        private readonly IQueryBuilderConfigurator _configurator;
        private readonly IDistributedCache _cache;

        public RedisQueryBuilderProvider(IQueryBuilderConfigurator configurator, 
            IDistributedCache cache, IHttpContextAccessor accessor, 
            RedlockFactoryProvider lockFactoryProvider) 
            : base(accessor, lockFactoryProvider)
        {
            _configurator = configurator;
            _cache = cache;
        }

        public bool SaveState => true;

        public QueryBuilder Get(string id)
        {
            return ExecuteWithLock(id, () => GetByCacheKey(GetCacheKey(id), id));
        }

        private QueryBuilder GetByCacheKey(string key, string id = null)
        {
            var layoutSql = _cache.GetString(key);
            if (string.IsNullOrEmpty(layoutSql))
            {
                var newQb = new QueryBuilder(id);
                newQb.Configuration(_configurator);
                return newQb;
            }

            _cache.Refresh(key);

            var qb = new QueryBuilder(id);
            qb.Configuration(_configurator);
            qb.LayoutSQL = layoutSql;

            return qb;
        }

        public void Put(QueryBuilder qb)
        {
            ExecuteWithLock(qb.Tag, () => _cache.SetString(GetCacheKey(qb.Tag), qb.LayoutSQL));
        }

        public void Delete(string id)
        {
            ExecuteWithLock(id, () => _cache.Remove(GetCacheKey(id)));
        }

        public bool CheckToken(string token, string instanceId)
        {
            if (token == null)
                return false;

            return !string.IsNullOrEmpty(_cache.GetString(BuildCacheKey(token, instanceId)));
        }

        public string CreateToken()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
