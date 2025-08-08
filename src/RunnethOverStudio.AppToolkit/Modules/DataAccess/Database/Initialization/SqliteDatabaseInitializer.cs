using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using RunnethOverStudio.AppToolkit.Core;
using RunnethOverStudio.AppToolkit.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using static RunnethOverStudio.AppToolkit.Core.Enums;

namespace RunnethOverStudio.AppToolkit.Modules.DataAccess;

/// <summary>
/// Provides methods to ensure the application's database is created and up-to-date.
/// </summary>
/// <remarks>
/// This implementation uses SQLite as the database engine and is designed to be thread-safe.
/// </remarks>
public class SqliteDatabaseInitializer : IDatabaseInitializer
{
    private static readonly object _initLock = new();

    private readonly ILogger _logger;
    private readonly IFileSystemAccess _fileSystemAccess;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteDatabaseInitializer"/> class with the specified logger.
    /// </summary>
    /// <param name="logger">The logger used to record informational messages, warnings, and errors related to database initialization.</param>
    /// <param name="fileSystemAccess">The utility for interacting with the operating system's files and directories.</param>
    public SqliteDatabaseInitializer(ILogger<IDatabaseInitializer> logger, IFileSystemAccess fileSystemAccess)
    {
        _logger = logger;
        _fileSystemAccess = fileSystemAccess;
    }

    /// <inheritdoc/>
    public ProcessResult<string> GetDBPath()
    {
        ProcessResult<string> appDirectoryResult = _fileSystemAccess.GetAppDirectoryPath();
        if (!appDirectoryResult.IsValid)
        {
            _logger.LogError("Failed to retrieve path to the application's SQLite database file.");
            return appDirectoryResult;
        }

        try
        {
            string appFolderPath = _fileSystemAccess.GetAppDirectoryPath().Content;
            string appName = new DirectoryInfo(appFolderPath).Name;
            string dbPath = Path.Join(appFolderPath, $"{appName}.db"); ;
            return ProcessResult<string>.Success(dbPath);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to retrieve path to the application's SQLite database file.");
            return ProcessResult<string>.Failure(string.Empty, StatusCodes.InternalError, ex);
        }
    }

    /// <inheritdoc/>
    public ProcessResult<bool> InitializeDatabase()
    {
        lock (_initLock)
        {
            ProcessResult<string> dbPathResult = GetDBPath();
            if (!dbPathResult.IsValid)
            {
                _logger.LogError("Failed to initialize database.");
                return ProcessResult<bool>.Failure(false, StatusCodes.InternalError, [.. dbPathResult.Errors]);
            }

            try
            {
                string dbPath = dbPathResult.Content;

                if (File.Exists(dbPath))
                {
                    UpdateExistingSqliteDB(dbPath);
                }
                else
                {
                    CreateNewSqliteDB(dbPath);
                }

                return ProcessResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to initialize database.");
                return ProcessResult<bool>.Failure(false, StatusCodes.InternalError, ex);
            }
        }
    }

    private static void CreateNewSqliteDB(string dbPath)
    {
        using SqliteConnection connection = new($"Data Source={dbPath}");
        connection.Open();

        // Create Migration table
        const string createMigrationTable = @"
            CREATE TABLE IF NOT EXISTS Migration (
                MigrationId INTEGER PRIMARY KEY AUTOINCREMENT,
                Number INTEGER NOT NULL,
                AppliedOn TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP)
            );
        ";

        connection.Execute(createMigrationTable);

        RunMigrations(connection, 1);
    }

    private static void UpdateExistingSqliteDB(string dbPath)
    {
        using SqliteConnection connection = new($"Data Source={dbPath}");
        connection.Open();

        const string sql = "SELECT MAX(Number) FROM Migration;";
        uint lastMigrationNumber = connection.ExecuteScalar<uint>(sql);

        RunMigrations(connection, ++lastMigrationNumber);
    }

    private static void RunMigrations(DbConnection connection, uint startingNumber)
    {
        List<BaseMigration> migrations = [];

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type assemblyType in assembly.GetTypes())
            {
                if (!assemblyType.IsAbstract && typeof(BaseMigration).IsAssignableFrom(assemblyType))
                {
                    string name = assemblyType.Name;
                    string numPart = name.GetAfterLast('_');

                    if (uint.TryParse(numPart, out uint migrationNumber) && migrationNumber >= startingNumber)
                    {
                        migrations.Add((BaseMigration)Activator.CreateInstance(assemblyType)!);
                    }
                }
            }
        }

        foreach (BaseMigration migration in migrations.OrderBy(m => m.Number()))
        {
            migration.Run(connection);
        }
    }
}
