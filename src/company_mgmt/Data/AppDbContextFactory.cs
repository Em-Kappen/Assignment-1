using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace company_mgmt.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=companydb;Username=postgres;SslMode=Disable";
        var connectionBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            SslMode = Enum.TryParse<SslMode>(
                Environment.GetEnvironmentVariable("Database__SslMode"),
                ignoreCase: true,
                out var sslMode)
                ? sslMode
                : new NpgsqlConnectionStringBuilder(connectionString).SslMode
        };
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionBuilder.ConnectionString)
            .Options;
        return new AppDbContext(options);
    }
}
