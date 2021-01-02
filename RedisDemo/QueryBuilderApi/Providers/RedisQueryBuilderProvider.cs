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
    public class RedisQueryBuilderProvider : IQueryBuilderProvider
    {
        private readonly IQueryBuilderConfigurator _configurator;
        private readonly IDistributedCache _cache;
        private readonly IHttpContextAccessor _accessor;
        private readonly RedLockFactory _lockFactory;

        private HttpContext Context
        {
            get { return _accessor.HttpContext; }
        }

        public RedisQueryBuilderProvider(IQueryBuilderConfigurator configurator, IDistributedCache cache, IHttpContextAccessor accessor,
            RedlockFactoryProvider lockFactoryProvider)
        {
            _configurator = configurator;
            _cache = cache;
            _accessor = accessor;
            _lockFactory = lockFactoryProvider.Factory;
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

        private void ExecuteWithLock(string instanceId, Action action)
        {
            using (var @lock = GetLock(instanceId))
            {
                if (@lock.IsAcquired)
                    action();
            }
        }

        private T ExecuteWithLock<T>(string instanceId, Func<T> func)
        {
            using (var @lock = GetLock(instanceId))
            {
                if (@lock.IsAcquired)
                    return func();

                return default;
            }
        }

        private IRedLock GetLock(string instanceId)
        {
            var key = GetCacheKey(instanceId);
            return _lockFactory.CreateLock($"{key}-lock", TimeSpan.FromSeconds(30));
        }

        public string GetSql(GetSqlModel model)
        { 
            var qb = GetByCacheKey(model.Token);
            if (qb == null)
                return null;

            var qt = new QueryTransformer { QueryProvider = qb.SQLQuery };

            qt.Skip((model.Pagenum * model.Pagesize).ToString());
            qt.Take(model.Pagesize == 0 ? "" : model.Pagesize.ToString());

            if (!string.IsNullOrEmpty(model.Sortdatafield))
            {
                qt.Sortings.Clear();

                if (!string.IsNullOrEmpty(model.Sortorder))
                {
                    var c = qt.Columns.FindColumnByResultName(model.Sortdatafield);

                    if (c != null)
                        qt.OrderBy(c, model.Sortorder.ToLowerInvariant() == "asc");
                }
            }

            return qt.SQL;
        }

        private string GetCacheKey(string id)
        {
            return BuildCacheKey(GetToken(), id);
        }

        private string BuildCacheKey(string token, string id)
        {
            return $"{token}:{id}";
        }

        private string GetToken()
        {
            var token = Context.Request.Headers["query-builder-token"];
            if (string.IsNullOrEmpty(token))
                throw new ApplicationException("Token not found");

            return token;
        }
    }
}
