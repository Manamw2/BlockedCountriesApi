using Microsoft.AspNetCore.Http;

namespace BlockedCountriesApi.Helpers
{
    public static class HttpContextHelper
    {
        /// <summary>
        /// Gets the client's IP address from HttpContext
        /// Handles X-Forwarded-For and X-Real-IP headers for proxies/load balancers
        /// </summary>
        /// <param name="httpContext">The HTTP context</param>
        /// <returns>The client's IP address</returns>
        public static string GetClientIpAddress(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            // Check X-Forwarded-For header (used by proxies/load balancers)
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                // X-Forwarded-For can contain multiple IPs, take the first one (client IP)
                var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (ips.Length > 0)
                {
                    return ips[0].Trim();
                }
            }

            // Check X-Real-IP header (alternative header used by some proxies)
            var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(realIp))
            {
                return realIp.Trim();
            }

            // Fallback to RemoteIpAddress
            var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrWhiteSpace(remoteIp))
            {
                // Handle IPv6 loopback
                if (remoteIp == "::1")
                {
                    return "127.0.0.1";
                }

                return remoteIp;
            }

            // Default fallback
            return "0.0.0.0";
        }

        /// <summary>
        /// Gets the User-Agent header from the HTTP request
        /// </summary>
        /// <param name="httpContext">The HTTP context</param>
        /// <returns>The User-Agent string, or "Unknown" if not available</returns>
        public static string GetUserAgent(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var userAgent = httpContext.Request.Headers["User-Agent"].FirstOrDefault();
            return string.IsNullOrWhiteSpace(userAgent) ? "Unknown" : userAgent;
        }
    }
}

