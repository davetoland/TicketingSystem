using System;
using System.Linq;
using System.Security.Claims;

namespace Aareon.Api
{
    public static class Extensions
    {
        public static int GetUserId(this ClaimsPrincipal principle)
        {
            var sid = principle.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Sid);
            if (sid == null)
                throw new InvalidOperationException("Claims.Sid cannot be null");
            
            return int.Parse(sid.Value);
        }
    }
}