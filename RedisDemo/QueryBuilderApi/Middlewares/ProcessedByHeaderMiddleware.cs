using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QueryBuilderApi.Middlewares
{
    public class ProcessedByHeaderMiddleware
    {
        private readonly RequestDelegate _next;
        private static string AppId = new Random().Next(1000).ToString();

        public ProcessedByHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers.TryAdd("Processed-By-Id", AppId);
            await _next(context);
        }
    }
}
