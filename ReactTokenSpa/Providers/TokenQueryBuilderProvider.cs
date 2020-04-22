using System;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Infrastructure.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace ReactTokenSpa.Providers
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
            return GetCacheItem(GetToken())?.QueryBuilder;
        }

        public void Put(QueryBuilder qb)
        {
            var cacheItem = GetCacheItem(GetToken());
            if (cacheItem != null)
                cacheItem.QueryBuilder = qb;
        }

        public void Delete(string id)
        {
            GetCacheItem(GetToken())?.Delete();
        }

        private void CreateItem(string token)
        {
            if (CheckToken(token))
                GetCacheItem(token).Dispose();

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(20))
                .RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    (value as TokenStoreItem)?.Dispose();
                })
                .SetPriority(CacheItemPriority.NeverRemove);

            _cache.Set(token, new TokenStoreItem(), cacheEntryOptions);
        }

        private TokenStoreItem GetCacheItem(string token)
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
        public QueryBuilder QueryBuilder { get; set; }

        public void Delete()
        {
            QueryBuilder?.Dispose();
            QueryBuilder = null;
        }

        public void Dispose()
        {
            QueryBuilder?.Dispose();
            QueryBuilder = null;
        }
    }
}