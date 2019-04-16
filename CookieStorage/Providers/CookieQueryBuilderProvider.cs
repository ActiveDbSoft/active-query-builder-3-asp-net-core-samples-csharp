using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ActiveQueryBuilder.Core;
using ActiveQueryBuilder.Web.Core;
using ActiveQueryBuilder.Web.Server;
using ActiveQueryBuilder.Web.Server.Infrastructure.Providers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;

namespace AspNetCoreCookieStorage.Providers
{
    public class CookieQueryBuilderProvider : IQueryBuilderProvider
    {
        public bool SaveState { get; private set; } = true;
    
        private readonly IHostingEnvironment _env;
        private readonly IHttpContextAccessor _contextAccessor;
        
        public CookieQueryBuilderProvider(IHostingEnvironment env, IHttpContextAccessor contextAccessor)
        {
            _env = env;
            _contextAccessor = contextAccessor;
        }

        public QueryBuilder Get(string id)
        {
            var qb = Create();
            LoadMetadata(qb);
            LoadState(qb);
            return qb;
        }

        private QueryBuilder Create()
        {
            return new QueryBuilder
            {
                SyntaxProvider = new MSSQLSyntaxProvider(),
                BehaviorOptions =
                {
                    AllowSleepMode = true
                },
                MetadataLoadingOptions =
                {
                    OfflineMode = true
                }
            };
        }

        private void LoadMetadata(QueryBuilder qb)
        {
            var path = @"../../Sample databases/Northwind.xml";
            var xml = Path.Combine(_env.WebRootPath, path);

            qb.MetadataContainer.ImportFromXML(xml);
        }

        private void LoadState(QueryBuilder qb)
        {
            var state = GetState();

            if (!string.IsNullOrEmpty(state))
                qb.LayoutSQL = state;
        }

        public void Put(QueryBuilder qb)
        {
            SetState(qb.LayoutSQL);
        }

        public void Delete(string id)
        {

        }

        private string GetState()
        {
            return GetContext().Request.Cookies["QueryBuilderState"];
        }

        private void SetState(string state)
        {
            SetCookie("QueryBuilderState", state);
        }

        private void SetCookie(string key, string value)
        {
            var context = GetContext();
            context.Response.Cookies.Append(key, value);
        }

        private HttpContext GetContext()
        {
            return _contextAccessor.HttpContext;
        }
    }
}
