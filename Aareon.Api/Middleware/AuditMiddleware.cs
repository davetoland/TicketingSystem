using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Aareon.Api.Middleware
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private const string AuditInTemplate = "Audit: [{0}] {1} Request to {2} received from {3} ({4}) [{5}]";
        private const string AuditOutTemplate = "Audit: [{0}] {1} Response sent in {2} seconds";

        public AuditMiddleware(RequestDelegate next, ILogger logger)
        {
            _next = next;
            _logger = logger;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            var uid = Guid.NewGuid().ToString();
            var host = context.Request.Host.Host;
            var path = context.Request.Path;
            var ip = context.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            var agent = string.Join(" ", context.Request?.Headers["User-Agent"]);
            var inTime = DateTime.Now;
            _logger.LogTrace(AuditInTemplate, uid, inTime.ToLongTimeString(), path, host, ip, agent);
            await _next(context);
            var duration = Math.Round((DateTime.Now - inTime).TotalSeconds, 2);
            _logger.LogTrace(AuditOutTemplate, uid, inTime.ToLongTimeString(), duration);
        }
    }
    
    public static class AuditMiddlewareExtension {  
        public static IApplicationBuilder UseAuditing(this IApplicationBuilder builder) {  
            return builder.UseMiddleware<AuditMiddleware>();  
        }  
    }
}