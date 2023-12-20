using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Twileloop.WebEmbed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .PartManager.ApplicationParts.Add(new AssemblyPart(typeof(PerfomanceController).Assembly));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UsePerformanceMiddleware(new PerformanceMiddlewareOptions
{
    FolderName = "WebRoot"
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
