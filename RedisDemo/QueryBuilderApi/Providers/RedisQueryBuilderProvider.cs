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
            var key = GetCacheKey(id);
            return ExecuteWithLock(key, () => GetByCacheKey(key, id));
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
            var key = GetCacheKey(qb.Tag);
            ExecuteWithLock(key, () => _cache.SetString(key, qb.LayoutSQL));
        }

        public void Delete(string id)
        {
            var key = GetCacheKey(id);
            ExecuteWithLock(key, () => _cache.Remove(key));
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
