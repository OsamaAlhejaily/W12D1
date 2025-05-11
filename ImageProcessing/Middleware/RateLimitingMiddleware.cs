using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageProcessingApi.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly int _limit;
        private readonly int _intervalSeconds;
        private readonly ConcurrentDictionary<string, List<DateTime>> _requests = new();

        public RateLimitingMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            // Read the maximum number of allowed requests per interval from config (e.g., appsettings.json)
            _limit = int.Parse(config["RateLimiting:Limit"]);
            // Read the interval duration in seconds from config
            _intervalSeconds = int.Parse(config["RateLimiting:IntervalInSeconds"]);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Extract the API key from the request headers
            var key = context.Request.Headers["X-Api-Key"].ToString();

            // Capture the current UTC timestamp
            var now = DateTime.UtcNow;

            // Get the list of timestamps for this key, or create a new one if it doesn't exist
            var requests = _requests.GetOrAdd(key, _ => new List<DateTime>());

            // Lock the list to make it thread-safe
            lock (requests)
            {
                // Remove timestamps older than the allowed interval
                var cutoff = now.AddSeconds(-_intervalSeconds);
                requests.RemoveAll(timestamp => timestamp < cutoff);

                // Check if the number of recent requests exceeds the limit
                if (requests.Count >= _limit)
                {
                    // Respond with HTTP 429 Too Many Requests
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    // Tell the client how long to wait before retrying
                    context.Response.Headers.Add("Retry-After", _intervalSeconds.ToString());
                    context.Response.WriteAsync("Rate limit exceeded.");

                    return; // End the request here
                }

                // Add the current request time to the list
                requests.Add(now);
            }
            await _next(context);
        }
    }
}