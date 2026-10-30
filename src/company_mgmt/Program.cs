using System.Diagnostics;
using company_mgmt.Data;
using company_mgmt.Models;
using company_mgmt.Services;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.ModelBuilder;

var builder = WebApplication.CreateBuilder(args);
var connectionString = BuildConnectionString(builder.Configuration);
var benchmarkBaseAddress = BuildBenchmarkBaseAddress(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlServer => sqlServer.CommandTimeout(1800))
        .EnableDetailedErrors()
        .LogTo(Console.WriteLine, LogLevel.Information));
builder.Services.AddScoped<IDatabaseSetupService, DatabaseSetupService>();
builder.Services.AddScoped<IApiBenchmarkService, ApiBenchmarkService>();
builder.Services.AddHttpClient("BenchmarkTarget", client =>
{
    client.BaseAddress = benchmarkBaseAddress;
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler(options => { });
builder.Services.AddOpenApi();

var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<Customer>("Customers");
modelBuilder.EntitySet<Product>("Products");
modelBuilder.EntitySet<Order>("Orders").EntityType.HasKey(order => order.SalesOrderGenerationId);
modelBuilder.EntitySet<OrderItem>("OrderItems").EntityType.HasKey(item => item.SalesOrderLineGenerationId);

builder.Services.AddControllers().AddOData(options =>
{
    options.AddRouteComponents("odata", modelBuilder.GetEdmModel())
        .Select()
        .Expand()
        .Filter()
        .OrderBy()
        .Count()
        .SkipToken()
        .SetMaxTop(10000);
});

var app = builder.Build();

app.UseExceptionHandler();
app.Use(async (context, next) =>
{
    var stopwatch = Stopwatch.StartNew();
    try
    {
        await next();
    }
    finally
    {
        stopwatch.Stop();
        app.Logger.LogInformation(
            "HTTP {Method} {Path} returned {StatusCode} in {ElapsedMs} ms",
            context.Request.Method, context.Request.Path, context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapGet("/demo/{**path}", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "index.html"));
});
app.Run();

static string BuildConnectionString(ConfigurationManager configuration)
{
    return configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "ConnectionStrings:DefaultConnection must be configured before starting the application.");
}

static Uri BuildBenchmarkBaseAddress(ConfigurationManager configuration)
{
    var configuredAddress = configuration["BenchmarkTests:BaseUrl"] ?? "https://localhost:7046/";
    if (!Uri.TryCreate(configuredAddress, UriKind.Absolute, out var address)
        || address.Scheme is not ("http" or "https")
        || !address.IsLoopback
        || !string.IsNullOrEmpty(address.UserInfo)
        || address.AbsolutePath is not ("" or "/")
        || !string.IsNullOrEmpty(address.Query)
        || !string.IsNullOrEmpty(address.Fragment))
    {
        throw new InvalidOperationException(
            "BenchmarkTests:BaseUrl must be an HTTP(S) localhost or loopback origin without a path, credentials, query, or fragment.");
    }

    return new Uri(address.GetLeftPart(UriPartial.Authority) + "/", UriKind.Absolute);
}
