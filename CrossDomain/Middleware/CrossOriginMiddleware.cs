using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AspNetCoreCrossDomain.Middleware
{
    public class CrossOriginMiddleware
    {
        private readonly RequestDelegate _next;

        public CrossOriginMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (BeginInvoke(context))
                return;

            await _next.Invoke(context);
            EndInvoke(context);
        }

        private bool BeginInvoke(HttpContext context)
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = "http://localhost:9080";
            context.Response.Headers["Access-Control-Allow-Methods"] = "*";

            context.Response.Headers["Access-Control-Allow-Headers"] =  "query-builder-token";

            if (context.Request.Method == "OPTIONS")
            {
                context.Response.WriteAsync("");
                return true;
            }

            return false;
        }

        private void EndInvoke(HttpContext context)
        {

        }
    }
}
