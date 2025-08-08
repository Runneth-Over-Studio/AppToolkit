using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using RunnethOverStudio.AppToolkit.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace RunnethOverStudio.AppToolkit.Modules.DataAccess;

/// <summary>
/// Generic asynchronous CRUD operations that execute against a SQL database.
/// </summary>
/// <remarks>
/// This implementation uses Dapper for access to the app's SQLite database.
/// </remarks>
public class DapperSQLiteDataAccess : ISQLDataAccess
{
    //Note: At the time of writing, Dapper query execution methods do not support cancellation.

    private readonly ILogger _logger;
    private readonly string _dbPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="DapperSQLiteDataAccess"/> class.
    /// </summary>
    /// <param name="logger">The logger used to record informational messages, warnings, and errors related to database access operations.</param>
    /// <param name="databaseInitializer">Initializer of the application's database.</param>
    public DapperSQLiteDataAccess(ILogger<ISQLDataAccess> logger, IDatabaseInitializer databaseInitializer)
    {
        ProcessResult<string> dbPathResult = databaseInitializer.GetDBPath();

        if (!dbPathResult.IsValid)
        {
            Exception badPathEx = new("Unable to determine path to database. Cannot continue with establishing data access.");
            logger.LogError(badPathEx, "Database path retrieval failed.");
            throw badPathEx;
        }

        if (!databaseInitializer.InitializeDatabase().IsValid)
        {
            Exception badInitEx = new("Database initialization failed. Cannot continue with establishing data access.");
            logger.LogError(badInitEx, "Database initialization failed.");
            throw badInitEx;
        }

        _logger = logger;
        _dbPath = dbPathResult.Content;
    }

    /// <inheritdoc/>
    public async Task<long?> CreateAsync<T>(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            Type type = typeof(T);
            string tableName = type.Name;
            List<PropertyInfo> properties = [.. type.GetProperties()
                .Where(p => !p.Name.Equals($"{tableName}Id", StringComparison.OrdinalIgnoreCase))];

            string columns = string.Join(", ", properties.Select(p => p.Name));
            string values = string.Join(", ", properties.Select(p => "@" + p.Name));
            string sql = $"INSERT INTO {tableName} ({columns}) VALUES ({values}); SELECT last_insert_rowid();";

            await using SqliteConnection conn = CreateConnection();
            await conn.OpenAsync(cancellationToken);

            return await conn.ExecuteScalarAsync<long?>(sql, entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateAsync<{EntityType}>.", typeof(T).Name);
            throw; //TODO: Return a ProcessResult<long> instead.
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<dynamic>> ReadRawAsync(string sql, object? parameters = null, CancellationToken cancellationToken = default)
    {
        try
        {
            await using SqliteConnection conn = CreateConnection();
            await conn.OpenAsync(cancellationToken);

            return await conn.QueryAsync(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ReadRawAsync. SQL: {Sql}", sql);
            throw; //TODO: Return a ProcessResult<IEnumerable<dynamic>> instead.
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> ReadAsync<T>(string? whereClause = null, object? parameters = null, CancellationToken cancellationToken = default, params string[]? columns)
    {
        try
        {
            string tableName = typeof(T).Name;
            string selectColumns = columns != null && columns.Length > 0 ? string.Join(", ", columns) : "*";
            string sql = $"SELECT {selectColumns} FROM {tableName}";
            if (!string.IsNullOrWhiteSpace(whereClause))
            {
                sql += $" WHERE {whereClause}";
            }

            await using SqliteConnection conn = CreateConnection();
            await conn.OpenAsync(cancellationToken);
            return await conn.QueryAsync<T>(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ReadAsync<{EntityType}>. Where: {WhereClause}", typeof(T).Name, whereClause);
            throw; //TODO: Return a ProcessResult<IEnumerable<T>> instead.
        }
    }

    /// <inheritdoc/>
    public async Task<T?> ReadByPrimaryKeyAsync<T>(long key, CancellationToken cancellationToken = default, params string[]? columns)
    {
        try
        {
            string tableName = typeof(T).Name;
            string idColumn = $"{tableName}Id";
            string selectColumns = columns != null && columns.Length > 0 ? string.Join(", ", columns) : "*";
            string sql = $"SELECT {selectColumns} FROM {tableName} WHERE {idColumn} = @id LIMIT 1;";

            await using SqliteConnection conn = CreateConnection();
            await conn.OpenAsync(cancellationToken);

            return await conn.QuerySingleOrDefaultAsync<T>(sql, new { id = key });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ReadByPrimaryKeyAsync<{EntityType}>. Key: {Key}", typeof(T).Name, key);
            throw; //TODO: Return a ProcessResult<T?> instead.
        }
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateByPrimaryKeyAsync<T>(long key, IDictionary<string, object?> columnValues, CancellationToken cancellationToken = default)
    {
        try
        {
            if (columnValues.Count == 0)
            {
                throw new ArgumentException("At least one column value must be provided.", nameof(columnValues));
            }

            string tableName = typeof(T).Name;
            string idColumn = $"{tableName}Id";
            string setClause = string.Join(", ", columnValues.Keys.Select(col => $"{col} = @{col}"));
            string sql = $"UPDATE {tableName} SET {setClause} WHERE {idColumn} = @id;";

            DynamicParameters parameters = new();
            parameters.Add("id", key);
            foreach (var kvp in columnValues)
            {
                parameters.Add(kvp.Key, kvp.Value);
            }

            await using SqliteConnection conn = CreateConnection();
            await conn.OpenAsync(cancellationToken);

            int affected = await conn.ExecuteAsync(sql, parameters);
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateByPrimaryKeyAsync<{EntityType}>. Key: {Key}", typeof(T).Name, key);
            throw; //TODO: Return a ProcessResult<bool> instead.
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteByPrimaryKeyAsync<T>(long key, CancellationToken cancellationToken = default)
    {
        try
        {
            string tableName = typeof(T).Name;
            string idColumn = $"{tableName}Id";
            string sql = $"DELETE FROM {tableName} WHERE {idColumn} = @id;";

            await using SqliteConnection conn = CreateConnection();
            await conn.OpenAsync(cancellationToken);

            int affected = await conn.ExecuteAsync(sql, new { id = key });
            return affected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteByPrimaryKeyAsync<{EntityType}>. Key: {Key}", typeof(T).Name, key);
            throw; //TODO: Return a ProcessResult<bool> instead.
        }
    }

    private SqliteConnection CreateConnection() => new($"Data Source={_dbPath}");
}
