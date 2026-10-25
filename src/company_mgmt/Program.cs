using company_mgmt.Data;
using company_mgmt.Models;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.ModelBuilder;
using System.Data;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection must be configured before starting the application.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<Employee>("Employees");
modelBuilder.EntitySet<Product>("Products");
modelBuilder.EntitySet<Branch>("Branches");

builder.Services.AddControllers().AddOData(options => {
    options.AddRouteComponents("odata", modelBuilder.GetEdmModel())
           .Select()
           .Filter()
           .OrderBy()
           .SkipToken()
           .SetMaxTop(100);
});

var app = builder.Build();

await EnsureDatabaseAsync(app.Services);

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.Run();

static async Task EnsureDatabaseAsync(IServiceProvider services) {
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var connection = db.Database.GetDbConnection();

    if (connection.State != ConnectionState.Open) {
        await connection.OpenAsync();
    }

    using var command = connection.CreateCommand();
    command.CommandText = @"
        SELECT EXISTS (
            SELECT 1
            FROM information_schema.tables
            WHERE table_schema = 'public' AND table_name = 'branches'
        );";

    var result = await command.ExecuteScalarAsync();
    var schemaExists = result switch {
        bool b => b,
        string s => bool.TryParse(s, out var parsed) && parsed,
        _ => Convert.ToBoolean(result)
    };

    if (!schemaExists) {
        await db.Database.ExecuteSqlRawAsync(ReadEmbeddedSql("company_mgmt.Database.schema.sql"));
    }

    var hasData = await db.Branches.AnyAsync()
        || await db.Employees.AnyAsync()
        || await db.Products.AnyAsync();

    if (!hasData) {
        await db.Database.ExecuteSqlRawAsync(ReadEmbeddedSql("company_mgmt.Database.seed.sql"));
    }
}

static string ReadEmbeddedSql(string resourceName) {
    using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
        ?? throw new InvalidOperationException($"Embedded SQL resource '{resourceName}' was not found.");
    using var reader = new StreamReader(stream);
    return reader.ReadToEnd();
}