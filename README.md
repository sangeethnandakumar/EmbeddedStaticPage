# Embed UI + API from NuGet package

## Add Framework Reference To Your Class Library
This is required to access `IWebHostEnvironment`

```xml
	<ItemGroup>
		<FrameworkReference Include="Microsoft.AspNetCore.App" />
	</ItemGroup>
```

## Create a WebRoot folder and embed it

> NOTE: This will allow to embed all files inside WebRoot as well as subfolders

```xml
	<!-- Add this ItemGroup to embed your static files -->
	<ItemGroup>
		<EmbeddedResource Include="WebRoot\**\*.*" />
	</ItemGroup>
```

## Add your controllers. Use [ApiExplorer] to omit it from Swagger

> `[ApiExplorerSettings(IgnoreApi = true)]` only blocks the controller's visibility to Swagger. The controller will still be active

```csharp
using Microsoft.AspNetCore.Mvc;

namespace Twileloop.WebEmbed
{
    [ApiController]
    [Route("[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class PerfomanceController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet(Name = "Perf")]
        public IActionResult Get()
        {
            return Ok(Enumerable.Range(1, 5).Select(index => new
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray());
        }
    }
}
```

## Place your assets inside WebRoot folder

## Create The Driver Middleware

> IMPORTANT: Embedded paths hould begin with your namespace name like "Twileloop.WebEmbed.WebRoot"

```csharp
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
```

## Register API Controllers In Main App

Register Controllers from our new NuGet package in main app

```csharp
builder.Services.AddControllers()
    .PartManager.ApplicationParts.Add(new AssemblyPart(typeof(PerfomanceController).Assembly));
```

## Register Middleware

```csharp
app.UsePerformanceMiddleware(new PerformanceMiddlewareOptions
{
    FolderName = "Twileloop.WebEmbed.WebRoot"
});
```
