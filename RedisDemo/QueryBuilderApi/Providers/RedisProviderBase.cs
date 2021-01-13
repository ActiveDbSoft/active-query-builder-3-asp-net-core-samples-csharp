using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using RedLockNet;
using RedLockNet.SERedis;

namespace QueryBuilderApi.Providers
{
    public abstract class RedisProviderBase
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly RedLockFactory _lockFactory;

        private HttpContext Context => _accessor.HttpContext;

        protected RedisProviderBase(IHttpContextAccessor accessor, 
            RedlockFactoryProvider lockFactoryProvider)
        {
            _accessor = accessor;
            _lockFactory = lockFactoryProvider.Factory;
        }

        protected void ExecuteWithLock(string instanceId, Action action)
        {
            using (var @lock = GetLock(instanceId))
            {
                if (@lock.IsAcquired)
                    action();
            }
        }

        protected T ExecuteWithLock<T>(string instanceId, Func<T> func)
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

        protected virtual string GetCacheKey(string id)
        {
            return BuildCacheKey(GetToken(), id);
        }

        protected string BuildCacheKey(string token, string id)
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
