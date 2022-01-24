using System.Security.Claims;
using AutoFixture;
using Microsoft.AspNetCore.Http;

namespace Aareon.Api.Tests
{
    public class TestBase
    {
        protected Fixture Fixture { get; } = new();
        
        protected const string ContextScheme = "https";
        protected const string ContextHost = "test-host";
        
        protected static HttpContext FakeHttpContext
        {
            get
            {
                var ctx = new DefaultHttpContext();
                ctx.Request.Scheme = ContextScheme;
                ctx.Request.Host = new HostString(ContextHost);
                ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Sid, "1"),
                    new Claim(ClaimTypes.Name, "TestUser")
                }, "TestAuth"));
                return ctx;
            }
        }
        
    }
}