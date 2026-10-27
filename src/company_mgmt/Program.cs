using System.Diagnostics;
using System.Text;
using company_mgmt.Data;
using company_mgmt.Models;
using company_mgmt.Services;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.ModelBuilder;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
var connectionString = BuildConnectionString(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, postgres => postgres.CommandTimeout(1800))
        .EnableDetailedErrors()
        .LogTo(Console.WriteLine, LogLevel.Information));
builder.Services.AddScoped<IDatabaseSetupService, DatabaseSetupService>();
builder.Services.AddScoped<ISeedService, SeedService>();
builder.Services.AddScoped<IPerformanceService, PerformanceService>();
builder.Services.AddScoped<IQueryHistoryService, QueryHistoryService>();
builder.Services.AddHttpClient("Benchmarks", client => client.Timeout = TimeSpan.FromMinutes(30));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler(options => { });
builder.Services.AddOpenApi();

var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<Customer>("Customers");
modelBuilder.EntitySet<Product>("Products");
modelBuilder.EntitySet<Order>("Orders");
modelBuilder.EntitySet<OrderItem>("OrderItems");
modelBuilder.EntitySet<Employee>("Employees");
modelBuilder.EntitySet<Branch>("Branches");

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

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapGet("/demo/{**path}", async context =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "demo.html"));
});
app.Run();

static string BuildConnectionString(ConfigurationManager configuration)
{
    var rawConnectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "ConnectionStrings:DefaultConnection must be configured before starting the application.");
    var dbBuilder = new NpgsqlConnectionStringBuilder(rawConnectionString);
    if (string.IsNullOrEmpty(dbBuilder.Password) &&
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PGPASSWORD")))
    {
        dbBuilder.Password = Environment.GetEnvironmentVariable("PGPASSWORD");
    }

    if (string.IsNullOrEmpty(dbBuilder.Password))
    {
        if (Console.IsInputRedirected)
            throw new InvalidOperationException("PostgreSQL password is not configured and no interactive console is available.");

        Console.Write($"Enter PostgreSQL password for '{dbBuilder.Username}': ");
        dbBuilder.Password = ReadPasswordFromConsole();
        Console.WriteLine();
    }

    if (string.IsNullOrEmpty(dbBuilder.Password))
        throw new InvalidOperationException("PostgreSQL password is required to connect to the configured database.");

    dbBuilder.SearchPath = "public";
    return dbBuilder.ConnectionString;
}

static string ReadPasswordFromConsole()
{
    var password = new StringBuilder();
    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter)
            return password.ToString();
        if (key.Key == ConsoleKey.Backspace)
        {
            if (password.Length > 0)
                password.Length--;
            continue;
        }
        if (key.KeyChar != '\0')
            password.Append(key.KeyChar);
    }
}
