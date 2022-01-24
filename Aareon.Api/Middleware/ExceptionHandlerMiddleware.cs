using System;
using System.Net;
using System.Threading.Tasks;
using Aareon.Business.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Aareon.Api.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger logger)
        {
            _next = next;
            _logger = logger;
        }
        
        // Update this with specific exception handling conditions, as a general rule
        // Exception should only result from an unhandled scenario, unless you specifically want
        // a 500 (Internal Server Error) to be reported in which case either specify it here or
        // just allow it to fallback to the standard Exception handler anyway.
        // For example, client request problems should throw something like an ArgumentException 
        // which is caught below and translated into a BadRequest response... 

        public async Task InvokeAsync(HttpContext context)
        {
            try
            { 
                await _next(context);
            }
            catch (InvalidPersonException ipe)
            {
                var msg = $"Create Ticket: Cannot find a Person with Id: {ipe.PersonId}";
                _logger.LogError(ipe, msg);
                context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(msg);
            }
            catch (InvalidTicketException ite)
            {
                var tid = ite.TicketId != null ? $"{ite.TicketId}" : "null";
                var msg = $"Create Ticket: Cannot find a Ticket with Id: {tid}";
                _logger.LogError(ite, msg);
                context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(msg);
            }
            catch (ArgumentException aex)
            {
                _logger.LogError(aex, "Bad Request");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Server Error");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
    }
    
    public static class ExceptionHandlerExtension {  
        public static IApplicationBuilder UseControllerExceptionHandler(this IApplicationBuilder builder) {  
            return builder.UseMiddleware<ExceptionHandlerMiddleware>();  
        }  
    }
}