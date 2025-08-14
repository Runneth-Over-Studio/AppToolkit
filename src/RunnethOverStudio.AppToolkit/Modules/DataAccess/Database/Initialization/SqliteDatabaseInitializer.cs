using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using RunnethOverStudio.AppToolkit.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Reflection;

namespace RunnethOverStudio.AppToolkit.Modules.DataAccess;

/// <summary>
/// Provides methods to ensure the application's database is created and up-to-date.
/// </summary>
/// <remarks>
/// This implementation uses SQLite as the database engine.
/// </remarks>
public class SqliteDatabaseInitializer : IDatabaseInitializer
{
    private const string DB_FILE_EXTENSION = ".db";

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
        if (!appDirectoryResult.IsSuccessful)
        {
            _logger.LogError("Failed to retrieve path to the application's database file.");
            return appDirectoryResult;
        }

        try
        {
            string appFolderPath = appDirectoryResult.Value;
            string appName = new DirectoryInfo(appFolderPath).Name;
            string dbPath = Path.Join(appFolderPath, $"{appName}.{DB_FILE_EXTENSION}"); ;
            return ProcessResult<string>.Success(dbPath);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to retrieve path to the application's database file.");
            return ProcessResult<string>.Failure(ex);
        }
    }

    /// <inheritdoc/>
    public ProcessResult<bool> InitializeDatabase()
    {
        lock (_initLock)
        {
            ProcessResult<string> dbPathResult = GetDBPath();
            if (!dbPathResult.IsSuccessful)
            {
                ProcessResult<bool>.LogAndForwardException("Failed to initialize database.", dbPathResult.Error, _logger);
            }

            try
            {
                string dbPath = dbPathResult.Value;

                if (File.Exists(dbPath))
                {
                    UpdateExistingDB(dbPath);
                }
                else
                {
                    CreateNewDB(dbPath);
                }

                return ProcessResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ProcessResult<bool>.LogAndForwardException("Failed to initialize database.", ex, _logger);
            }
        }
    }

    private static void CreateNewDB(string dbPath)
    {
        using SqliteConnection connection = new($"Data Source={dbPath}");
        connection.Open();

        string migrationTableCreationScript = SQLiteMigration.GetMigrationTableCreationScript();

        connection.Execute(migrationTableCreationScript);

        RunMigrations(connection, 0);
    }

    private static void UpdateExistingDB(string dbPath)
    {
        using SqliteConnection connection = new($"Data Source={dbPath}");
        connection.Open();

        const string sql = "SELECT MAX(Number) FROM Migration;";
        uint? lastMigrationNumber = connection.ExecuteScalar<uint>(sql);

        RunMigrations(connection, (lastMigrationNumber ?? 0) + 1);
    }

    private static void RunMigrations(DbConnection connection, uint startingNumber)
    {
        SortedList<uint, IMigration> migrations = [];

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type assemblyType in assembly.GetTypes())
            {
                if (!assemblyType.IsAbstract
                    && !assemblyType.IsInterface
                    && typeof(IMigration).IsAssignableFrom(assemblyType)
                    && Activator.CreateInstance(assemblyType) is IMigration migration)
                {
                    if (migration.Number >= startingNumber)
                    {
                        migrations.Add(migration.Number, migration);
                    }
                }
            }
        }

        foreach (IMigration migration in migrations.Values)
        {
            migration.Run(connection);
        }
    }
}
