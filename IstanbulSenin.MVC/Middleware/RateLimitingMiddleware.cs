using System.Collections.Concurrent;

namespace IstanbulSenin.MVC.Middleware
{
    /// Rate Limiting - Brute force saldırılarından koruma
    /// Her IP adresi için istek sayısını sınırlandırır5 dakika içinde 100 istek sınırı (ayarlanabilir)

    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private static readonly ConcurrentDictionary<string, RequestCounter> RequestCounts = new();

        private const int MaxRequestsPerWindow = 100;
        private const int WindowSizeMinutes = 5;

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            bool isAuthEndpoint = context.Request.Path.ToString().Contains("/account/login", StringComparison.OrdinalIgnoreCase) ||
                                  context.Request.Path.ToString().Contains("/account/register", StringComparison.OrdinalIgnoreCase);

            int limit = isAuthEndpoint ? 20 : MaxRequestsPerWindow;

            if (!IsAllowed(ipAddress, limit))
            {
                _logger.LogWarning("RATE LIMIT: IP {IpAddress} limit exceeded! Endpoint: {Path}", ipAddress, context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = "Çok fazla istek. Lütfen daha sonra tekrar deneyin." });
                return;
            }

            await _next(context);
        }

        private static bool IsAllowed(string ipAddress, int limit)
        {
            var now = DateTime.UtcNow;
            var windowStart = now.AddMinutes(-WindowSizeMinutes);

            var counter = RequestCounts.AddOrUpdate(ipAddress,
                new RequestCounter { FirstRequestTime = now, Count = 1 },
                (key, existing) =>
                {
                    if (existing.FirstRequestTime < windowStart)
                    {
                        return new RequestCounter { FirstRequestTime = now, Count = 1 };
                    }

                    existing.Count++;
                    return existing;
                });

            return counter.Count <= limit;
        }

        private class RequestCounter
        {
            public DateTime FirstRequestTime { get; set; }
            public int Count { get; set; }
        }
    }

    public static class RateLimitingExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}
