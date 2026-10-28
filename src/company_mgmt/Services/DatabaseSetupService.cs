using System.Data;
using System.Diagnostics;
using company_mgmt.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace company_mgmt.Services;

public sealed class DatabaseSetupService(
    AppDbContext context,
    ILogger<DatabaseSetupService> logger) : IDatabaseSetupService
{
    private static readonly string[] Tables =
        ["branches", "customers", "products", "orders", "order_items", "employees", "benchmark_runs", "query_history"];

    public Task<SetupExecutionResult> RunSchemaAsync(CancellationToken cancellationToken) =>
        ExecuteScriptAsync("schema.sql", "schema", cancellationToken);

    public Task<SetupExecutionResult> RunSeedAsync(CancellationToken cancellationToken) =>
        ExecuteScriptAsync("seed.sql", "seed", cancellationToken);

    public async Task<Dictionary<string, long>> GetTableCountsAsync(CancellationToken cancellationToken)
    {
        var counts = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        var connection = (NpgsqlConnection)context.Database.GetDbConnection();
        var wasOpen = connection.State == ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(cancellationToken);

        try
        {
            foreach (var table in Tables)
            {
                await using var command = new NpgsqlCommand($"SELECT count(*) FROM public.{table}", connection);
                var result = await command.ExecuteScalarAsync(cancellationToken);
                counts[table] = Convert.ToInt64(result);
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
        var connection = (NpgsqlConnection)context.Database.GetDbConnection();
        var wasOpen = connection.State == ConnectionState.Open;
        try
        {
            if (!wasOpen)
                await connection.OpenAsync(cancellationToken);

            await using var command = new NpgsqlCommand(
                """
                SELECT current_database(), pg_database_size(current_database()),
                       to_regclass('public.customers') IS NOT NULL
                       AND to_regclass('public.products') IS NOT NULL
                       AND to_regclass('public.orders') IS NOT NULL
                       AND to_regclass('public.order_items') IS NOT NULL
                       AND to_regclass('public.employees') IS NOT NULL
                       AND to_regclass('public.benchmark_runs') IS NOT NULL
                       AND to_regclass('public.query_history') IS NOT NULL
                """, connection);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            await reader.ReadAsync(cancellationToken);
            var databaseName = reader.GetString(0);
            var sizeBytes = reader.GetInt64(1);
            var schemaInitialized = reader.GetBoolean(2);
            await reader.CloseAsync();

            var counts = schemaInitialized
                ? await GetTableCountsAsync(cancellationToken)
                : Tables.ToDictionary(table => table, _ => 0L, StringComparer.OrdinalIgnoreCase);
            DateTimeOffset? lastRun = null;
            if (schemaInitialized)
            {
                await using var lastRunCommand = new NpgsqlCommand(
                    "SELECT max(executed_at) FROM public.benchmark_runs", connection);
                var lastRunValue = await lastRunCommand.ExecuteScalarAsync(cancellationToken);
                if (lastRunValue is not DBNull and not null)
                    lastRun = new DateTimeOffset(DateTime.SpecifyKind((DateTime)lastRunValue, DateTimeKind.Utc));
            }
            return new DatabaseSummary(
                databaseName,
                sizeBytes,
                counts,
                lastRun,
                true,
                schemaInitialized,
                schemaInitialized ? "Database schema is initialized." : "Database is reachable; run schema setup to initialize its tables.");
        }
        catch (PostgresException exception) when (exception.SqlState == "3D000")
        {
            var databaseName = new NpgsqlConnectionStringBuilder(connection.ConnectionString).Database ?? string.Empty;
            logger.LogInformation(
                "Configured PostgreSQL database {DatabaseName} does not exist yet; schema setup can create it.",
                databaseName);
            return new DatabaseSummary(
                databaseName,
                0,
                Tables.ToDictionary(table => table, _ => 0L, StringComparer.OrdinalIgnoreCase),
                null,
                false,
                false,
                "The configured database does not exist. Run schema setup to create it.");
        }
        catch (NpgsqlException exception) when (
            exception.Message.Contains(
                "No SSL enabled connection from this host is configured",
                StringComparison.OrdinalIgnoreCase))
        {
            var databaseName = new NpgsqlConnectionStringBuilder(connection.ConnectionString).Database ?? string.Empty;
            logger.LogWarning(
                exception,
                "PostgreSQL database {DatabaseName} rejected the required TLS connection because SSL is not enabled for this client.",
                databaseName);
            return new DatabaseSummary(
                databaseName,
                0,
                Tables.ToDictionary(table => table, _ => 0L, StringComparer.OrdinalIgnoreCase),
                null,
                false,
                false,
                "The PostgreSQL TLS settings do not match the server. Check Database:SslMode and the server's SSL configuration.");
        }
        finally
        {
            if (!wasOpen)
                await connection.CloseAsync();
        }
    }

    public async Task<IReadOnlyList<IndexSummary>> GetIndexSummaryAsync(CancellationToken cancellationToken)
    {
        return await context.Database.SqlQuery<IndexSummary>(
                $"""
                 SELECT schemaname AS "Schema", tablename AS "Table", indexname AS "IndexName",
                        indexdef AS "Definition"
                 FROM pg_indexes
                 WHERE schemaname = 'public'
                 ORDER BY tablename, indexname
                 """)
            .ToListAsync(cancellationToken);
    }

    private async Task<SetupExecutionResult> ExecuteScriptAsync(
        string fileName,
        string operation,
        CancellationToken cancellationToken)
    {
        var scriptPath = Path.Combine(AppContext.BaseDirectory, "Database", fileName);
        if (!File.Exists(scriptPath))
            throw new FileNotFoundException($"Required database script was not found: {scriptPath}", scriptPath);

        var script = await File.ReadAllTextAsync(scriptPath, cancellationToken);
        var before = operation == "seed" ? await GetTableCountsAsync(cancellationToken) : [];
        var wasSeeded = operation == "seed" && await SeedMarkerExistsAsync(cancellationToken);
        var stopwatch = Stopwatch.StartNew();
        logger.LogInformation("Starting database {Operation} script {ScriptPath}", operation, scriptPath);

        try
        {
            if (operation == "schema")
                await EnsureTargetDatabaseExistsAsync(cancellationToken);

            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            await context.Database.ExecuteSqlRawAsync(script, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Database {Operation} script failed", operation);
            throw;
        }
        stopwatch.Stop();

        var after = await GetTableCountsAsync(cancellationToken);
        var delta = after.ToDictionary(
            pair => pair.Key,
            pair => Math.Max(0, pair.Value - before.GetValueOrDefault(pair.Key)),
            StringComparer.OrdinalIgnoreCase);
        logger.LogInformation(
            "Completed database {Operation} script in {ElapsedMs} ms. New rows: {RowCounts}",
            operation, stopwatch.ElapsedMilliseconds, string.Join(", ", delta.Select(x => $"{x.Key}={x.Value}")));

        var seededRows = delta.Values.Sum();
        var skipped = operation == "seed" && (wasSeeded || seededRows == 0);
        return new SetupExecutionResult(
            operation,
            !skipped,
            DateTimeOffset.UtcNow,
            stopwatch.ElapsedMilliseconds,
            delta,
            skipped ? "Benchmark seed already exists; no duplicate rows were inserted." : "Completed successfully.");
    }

    private async Task EnsureTargetDatabaseExistsAsync(CancellationToken cancellationToken)
    {
        var targetBuilder = new NpgsqlConnectionStringBuilder(context.Database.GetConnectionString());
        var targetDatabase = targetBuilder.Database;
        if (string.IsNullOrWhiteSpace(targetDatabase))
            throw new InvalidOperationException("The PostgreSQL connection string must specify a target database.");

        var maintenanceBuilder = new NpgsqlConnectionStringBuilder(targetBuilder.ConnectionString)
        {
            Database = string.Equals(targetDatabase, "postgres", StringComparison.OrdinalIgnoreCase)
                ? "template1"
                : "postgres"
        };

        await using var maintenanceConnection = new NpgsqlConnection(maintenanceBuilder.ConnectionString);
        await maintenanceConnection.OpenAsync(cancellationToken);
        await using (var existsCommand = new NpgsqlCommand(
            "SELECT EXISTS (SELECT 1 FROM pg_database WHERE datname = @databaseName)",
            maintenanceConnection))
        {
            existsCommand.Parameters.AddWithValue("databaseName", targetDatabase);
            if ((bool)(await existsCommand.ExecuteScalarAsync(cancellationToken) ?? false))
                return;
        }

        var quotedDatabaseName = new NpgsqlCommandBuilder().QuoteIdentifier(targetDatabase);
        try
        {
            await using var createCommand = new NpgsqlCommand(
                $"CREATE DATABASE {quotedDatabaseName}",
                maintenanceConnection);
            await createCommand.ExecuteNonQueryAsync(cancellationToken);
            logger.LogInformation("Created PostgreSQL database {DatabaseName}", targetDatabase);
        }
        catch (PostgresException exception) when (exception.SqlState == "42P04")
        {
            logger.LogInformation(
                "PostgreSQL database {DatabaseName} was created concurrently by another setup request.",
                targetDatabase);
        }
    }

    private async Task<bool> SeedMarkerExistsAsync(CancellationToken cancellationToken)
    {
        if (!await context.Database.CanConnectAsync(cancellationToken))
            return false;
        var connection = (NpgsqlConnection)context.Database.GetDbConnection();
        var wasOpen = connection.State == ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = new NpgsqlCommand(
                "SELECT to_regclass('public.database_metadata') IS NOT NULL", connection);
            var exists = (bool)(await command.ExecuteScalarAsync(cancellationToken) ?? false);
            if (!exists)
                return false;
            await using var markerCommand = new NpgsqlCommand(
                "SELECT EXISTS (SELECT 1 FROM public.database_metadata WHERE key = 'BenchmarkSeedVersion')",
                connection);
            return (bool)(await markerCommand.ExecuteScalarAsync(cancellationToken) ?? false);
        }
        finally
        {
            if (!wasOpen)
                await connection.CloseAsync();
        }
    }
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