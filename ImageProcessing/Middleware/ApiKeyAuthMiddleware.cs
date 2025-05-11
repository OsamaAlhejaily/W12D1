using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using ImageProcessingApi.Controllers;

namespace ImageProcessingApi.Middleware
{
    public class ApiKeyAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiKeyAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Get the requested path from the incoming HTTP request
            var path = context.Request.Path.Value;

            // Bypass static files like index.html, CSS, JS, etc.
            if (path != null && (path.StartsWith("/index.html") ||
                                 path.StartsWith("/css") ||
                                 path.StartsWith("/js") ||
                                 path.StartsWith("/images") ||
                                 path.StartsWith("/favicon.ico") ||
                                 path.StartsWith("/swagger") ||
                                 path.StartsWith("/api/apikeys/generate")))
            {
                await _next(context);
                return;
            }

            // For other requests, check if the X-Api-Key header exists and is valid
            if (!context.Request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeader) ||
                string.IsNullOrEmpty(apiKeyHeader) ||
                !ApiKeysController.IsValidApiKey(apiKeyHeader))
            {
                // Unauthorized
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid or missing API Key.");
                // Stop further processing
                return;
            }

            // If the API key is valid, allow the request to continue through the middleware pipeline
            await _next(context);
        }
    }
}