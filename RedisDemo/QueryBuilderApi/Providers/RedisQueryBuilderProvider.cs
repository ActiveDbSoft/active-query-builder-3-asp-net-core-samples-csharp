using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Configuration;
using ActiveQueryBuilder.Web.Server.Infrastructure.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace RedisStorage.Providers
{
    public class RedisQueryBuilderProvider : IQueryBuilderProvider
    {
        private readonly IQueryBuilderConfigurator _configurator;
        private readonly IDistributedCache _cache;
        private readonly IHttpContextAccessor _accessor;

        private HttpContext Context
        {
            get { return _accessor.HttpContext; }
        }

        public RedisQueryBuilderProvider(IQueryBuilderConfigurator configurator, IDistributedCache cache, IHttpContextAccessor accessor)
        {
            _configurator = configurator;
            _cache = cache;
            _accessor = accessor;
        }

        public bool SaveState => true;

        public QueryBuilder Get(string id)
        {
            return GetByCacheKey(GetCacheKey(id), id);
        }

        private QueryBuilder GetByCacheKey(string key, string id = null)
        {
            var layoutSql = _cache.GetString(key);
            if (string.IsNullOrEmpty(layoutSql))
                return null;

            _cache.Refresh(key);

            var qb = new QueryBuilder(id);
            qb.Configuration(_configurator);
            qb.LayoutSQL = layoutSql;

            return qb;
        }

        public void Put(QueryBuilder qb)
        {
            _cache.SetString(GetCacheKey(qb.Tag), qb.LayoutSQL);
        }

        public void Delete(string id)
        {
            _cache.Remove(GetCacheKey(id));
        }

        public bool CheckToken(string token)
        {
            if (token == null)
                return false;

            return !string.IsNullOrEmpty(_cache.GetString(token));
        }

        public string CreateToken()
        {
            return Guid.NewGuid().ToString("N");
        }

        public string GetSql(string cacheKey)
        {
            return GetByCacheKey(cacheKey)?.SQL;
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

            if (!CheckToken(token))
                return null;

            return token;
        }
    }
}
