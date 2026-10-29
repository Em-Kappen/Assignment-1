using System.Data;
using company_mgmt.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace company_mgmt.Services;

public sealed class DatabaseSetupService(
    AppDbContext context,
    ILogger<DatabaseSetupService> logger) : IDatabaseSetupService
{
    private static readonly (string Name, string SqlName)[] Tables =
    [
        ("customer", "[eclipse].[customer]"),
        ("product", "[eclipse].[product]"),
        ("sales_order_generation", "[eclipse].[sales_order_generation]"),
        ("sales_order_line_generation", "[eclipse].[sales_order_line_generation]")
    ];

    public Task<SetupExecutionResult> RunSchemaAsync(CancellationToken cancellationToken) =>
        Task.FromException<SetupExecutionResult>(
            new NotSupportedException("SQL_main is an existing database; schema setup is disabled."));

    public Task<SetupExecutionResult> RunSeedAsync(CancellationToken cancellationToken) =>
        Task.FromException<SetupExecutionResult>(
            new NotSupportedException("Synthetic seed data is disabled for SQL_main."));

    public async Task<Dictionary<string, long>> GetTableCountsAsync(CancellationToken cancellationToken)
    {
        var counts = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        var connection = (SqlConnection)context.Database.GetDbConnection();
        var wasOpen = connection.State == ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(cancellationToken);

        try
        {
            foreach (var (name, sqlName) in Tables)
            {
                await using var command = new SqlCommand($"SELECT COUNT_BIG(*) FROM {sqlName}", connection);
                counts[name] = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
            }
        }
        finally
        {
            if (!wasOpen)
                await connection.CloseAsync();
        }

        return counts;
    }

    public async Task<DatabaseSummary> GetDatabaseSummaryAsync(CancellationToken cancellationToken)
    {
        var connection = (SqlConnection)context.Database.GetDbConnection();
        var wasOpen = connection.State == ConnectionState.Open;
        try
        {
            if (!wasOpen)
                await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(
                """
                SELECT DB_NAME(),
                       COALESCE((SELECT SUM(CONVERT(bigint, size)) * 8192 FROM sys.database_files), 0),
                       CASE WHEN OBJECT_ID(N'eclipse.customer', N'U') IS NOT NULL
                                  AND OBJECT_ID(N'eclipse.product', N'U') IS NOT NULL
                                  AND OBJECT_ID(N'eclipse.sales_order_generation', N'U') IS NOT NULL
                                  AND OBJECT_ID(N'eclipse.sales_order_line_generation', N'U') IS NOT NULL
                            THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END
                """, connection);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            await reader.ReadAsync(cancellationToken);
            var databaseName = reader.GetString(0);
            var sizeBytes = reader.GetInt64(1);
            var schemaInitialized = reader.GetBoolean(2);
            await reader.CloseAsync();

            var counts = schemaInitialized
                ? await GetTableCountsAsync(cancellationToken)
                : Tables.ToDictionary(table => table.Name, _ => 0L, StringComparer.OrdinalIgnoreCase);

            return new DatabaseSummary(
                databaseName,
                sizeBytes,
                counts,
                null,
                true,
                schemaInitialized,
                schemaInitialized
                    ? "The existing SQL_main customer, order, and product tables are available."
                    : "One or more expected SQL_main tables are unavailable.");
        }
        catch (SqlException exception) when (exception.Number is 4060 or 4064)
        {
            var databaseName = new SqlConnectionStringBuilder(connection.ConnectionString).InitialCatalog;
            logger.LogWarning(exception, "Configured SQL Server database {DatabaseName} is unavailable.", databaseName);
            return new DatabaseSummary(
                databaseName,
                0,
                Tables.ToDictionary(table => table.Name, _ => 0L, StringComparer.OrdinalIgnoreCase),
                null,
                false,
                false,
                "The configured SQL Server database is unavailable.");
        }
        finally
        {
            if (!wasOpen)
                await connection.CloseAsync();
        }
    }

    public async Task<IReadOnlyList<IndexSummary>> GetIndexSummaryAsync(CancellationToken cancellationToken) =>
        await context.Database.SqlQuery<IndexSummary>(
                $"""
                 SELECT SCHEMA_NAME(t.schema_id) AS [Schema],
                        t.name AS [Table],
                        i.name AS [IndexName],
                        CONCAT(i.type_desc, CASE WHEN i.is_unique = 1 THEN ' UNIQUE' ELSE '' END) AS [Definition]
                 FROM sys.indexes AS i
                 INNER JOIN sys.tables AS t ON i.object_id = t.object_id
                 WHERE i.name IS NOT NULL
                   AND SCHEMA_NAME(t.schema_id) = N'eclipse'
                   AND t.name IN (N'customer', N'product', N'sales_order_generation', N'sales_order_line_generation')
                 """)
            .OrderBy(index => index.Schema)
            .ThenBy(index => index.Table)
            .ThenBy(index => index.IndexName)
            .ToListAsync(cancellationToken);

}

public sealed record SetupExecutionResult(
    string Operation,
    bool Executed,
    DateTimeOffset ExecutedAt,
    long DurationMilliseconds,
    Dictionary<string, long> RowCounts,
    string Message);

public sealed record DatabaseSummary(
    string DatabaseName,
    long DatabaseSizeBytes,
    Dictionary<string, long> TableCounts,
    DateTimeOffset? LastBenchmarkExecution,
    bool Connected,
    bool SchemaInitialized,
    string StatusMessage);

public sealed record IndexSummary(string Schema, string Table, string IndexName, string Definition);
