using Microsoft.AspNetCore.Mvc;

namespace Aareon.Api.Tests.Utilities
{
    public static class ActionResultExtensions
    {
        public static T GetFromActionResult<T>(this IActionResult result) where T: class
        {
            return result switch
            {
                ObjectResult objResult => objResult.Value as T,
                _ => null
            };
        }
    }
}