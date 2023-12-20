using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Twileloop.WebEmbed
{
    public class PerformanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly StaticFileMiddleware _staticFileMiddleware;

        public PerformanceMiddleware(
            RequestDelegate next,
            IWebHostEnvironment hostingEnv,
            ILoggerFactory loggerFactory,
            PerformanceMiddlewareOptions options)
        {
            _next = next;

            _staticFileMiddleware = CreateStaticFileMiddleware(hostingEnv, loggerFactory, options);
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // Check if the URL starts with the specified base URL
            if (httpContext.Request.Path.StartsWithSegments("/performance"))
            {
                await _staticFileMiddleware.Invoke(httpContext);
                return;
            }

            // Continue to the next middleware if the URL doesn't match
            await _next(httpContext);
        }


        private StaticFileMiddleware CreateStaticFileMiddleware(
    IWebHostEnvironment hostingEnv,
    ILoggerFactory loggerFactory,
    PerformanceMiddlewareOptions options)
        {
            var assembly = typeof(PerformanceMiddleware).GetTypeInfo().Assembly;

            var staticFileOptions = new StaticFileOptions
            {
                RequestPath = "/performance",
                FileProvider = new EmbeddedFileProvider(assembly, options.FolderName),
            };

            // Use the StaticFileMiddleware constructor directly
            return new StaticFileMiddleware(
                async context => await _next(context),
                hostingEnv,
                Options.Create(staticFileOptions),
                loggerFactory);
        }

    }

    public class PerformanceMiddlewareOptions
    {
        public string FolderName { get; set; } = "Twileloop.WebEmbed.WebRoot";
    }

    public static class PerformanceMiddlewareExtensions
    {
        public static IApplicationBuilder UsePerformanceMiddleware(
            this IApplicationBuilder builder,
            PerformanceMiddlewareOptions options)
        {
            return builder.UseMiddleware<PerformanceMiddleware>(options);
        }
    }
}