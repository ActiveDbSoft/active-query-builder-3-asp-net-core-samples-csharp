using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;

namespace QueryBuilderApi.Providers
{
    public class RedlockFactoryProvider
    {
        public RedLockFactory Factory { get; }

        public RedlockFactoryProvider(string redisHost)
        {
            Factory = RedLockFactory.Create(new List<RedLockEndPoint>
            {
                new DnsEndPoint(redisHost, 6379)
            });
        }
    }
}
