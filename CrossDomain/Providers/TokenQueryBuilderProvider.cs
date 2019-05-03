using System;
using System.Collections.Generic;
using System.Web;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Infrastructure.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace AspNetCoreCrossDomain.Providers
{
    // Token-based QueryBuilder storage provider
    // Stores TokenStoreItems using values from HTTP request header as a key.
    public class TokenQueryBuilderProvider : IQueryBuilderProvider
    {
        public bool SaveState { get; private set; }

        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _accessor;

        private HttpContext Context
        {
            get { return _accessor.HttpContext; }
        }

        public TokenQueryBuilderProvider(IMemoryCache cache, IHttpContextAccessor accessor)
        {
            SaveState = false;

            _accessor = accessor;

            _cache = cache;
        }

        public QueryBuilder Get(string id)
        {
            var token = GetToken();

            if (token == null)
                return null;

            return GetFromCache(token).Get(id);
        }

        public void Put(QueryBuilder qb)
        {
            var token = GetToken();
            GetFromCache(token).Put(qb);
        }

        public void Delete(string id)
        {
            var token = GetToken();
            GetFromCache(token).Delete(id);
        }

        private void CreateItem(string token)
        {
            if (CheckToken(token))
                GetFromCache(token).Dispose();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(20))
                .RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    (value as TokenStoreItem)?.Dispose();
                })
                .SetPriority(CacheItemPriority.NeverRemove);

            _cache.Set(token, new TokenStoreItem(), cacheEntryOptions);
        }

        private TokenStoreItem GetFromCache(string token)
        {
            if (token == null)
                return null;

            if (_cache.TryGetValue(token, out var value))
                return value as TokenStoreItem;
            else
                return null;
        }

        public bool CheckToken(string token)
        {
            if (token == null)
                return false;

            return _cache.TryGetValue(token, out _);
        }

        public string CreateToken()
        {
            var token = Guid.NewGuid().ToString();
            CreateItem(token);
            return token;
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

    // Token-based storage item holding an instance of the QueryBuilder object
    public class TokenStoreItem : IDisposable
    {
        private readonly Dictionary<string, QueryBuilder> QueryBuilders = new Dictionary<string, QueryBuilder>();

        public QueryBuilder Get(string id)
        {
            if (!QueryBuilders.ContainsKey(id))
                return null;

            return QueryBuilders[id];
        }

        public void Put(QueryBuilder qb)
        {
            if (QueryBuilders.ContainsKey(qb.Tag) && QueryBuilders[qb.Tag] == qb)
                return;

            QueryBuilders[qb.Tag] = qb;
        }

        public void Delete(string id)
        {
            QueryBuilders[id].Dispose();
            QueryBuilders.Remove(id);
        }

        public void Dispose()
        {
            foreach (var key in QueryBuilders.Keys)
                Delete(key);
        }
    }
}