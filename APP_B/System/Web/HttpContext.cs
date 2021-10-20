using Microsoft.AspNetCore.Http;

namespace System.Web
{
    internal class HttpContext
    {
        public static object Current { get; internal set; }

        internal static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            throw new NotImplementedException();
        }
    }
}