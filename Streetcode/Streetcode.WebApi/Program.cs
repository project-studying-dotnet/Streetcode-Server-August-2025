using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.Services.BlobStorageService;
using Streetcode.DAL.Persistence;
using Streetcode.WebApi.Attributes;
using Streetcode.WebApi.Extensions;
using Streetcode.WebApi.Utils;
using Streetcode.DAL.Entities.Users;
using Streetcode.WebApi.Middlewares;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureApplication();

// Ocelot Basic setup
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddOcelot(); // single ocelot.json file in read-only mode
builder.Services
    .AddOcelot(builder.Configuration);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSwaggerServices();
builder.Services.AddCustomServices();
builder.Services.ConfigureBlob(builder);
builder.Services.ConfigurePayment(builder);
builder.Services.ConfigureInstagram(builder);
builder.Services.ConfigureSerilog(builder);
builder.Services.ConfigureJwt(builder);

// Connect extension method Identity
builder.Services.AddIdentityServices();

var app = builder.Build();

if (app.Environment.EnvironmentName == "Local")
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIv5 v1"));
}
else
{
    app.UseHsts();
}

await app.ApplyMigrations();

await app.SeedDataAsync();
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseCors();
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/dash");

if (app.Environment.EnvironmentName != "Local")
{
    RecurringJob.AddOrUpdate<BlobService>(
        "BlobService_CleanBlobStorage",
        b => b.CleanBlobStorage(),
        Cron.Monthly);
}

// BackgroundJob.Schedule<WebParsingUtils>(
//     wp => wp.ParseZipFileFromWebAsync(), TimeSpan.FromMinutes(1));

// RecurringJob.AddOrUpdate<WebParsingUtils>(
//     "WebParsingUtils_ParseZipFile",
//     wp => wp.ParseZipFileFromWebAsync(),
//     Cron.Monthly);

app.MapControllers();

// Add middlewares ocelot
await app.UseOcelot();

await app.RunAsync();

public partial class Program
{
}
