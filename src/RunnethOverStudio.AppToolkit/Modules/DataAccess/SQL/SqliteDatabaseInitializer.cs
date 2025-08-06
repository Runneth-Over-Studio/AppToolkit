using Dapper;
using Microsoft.Data.Sqlite;
using RunnethOverStudio.AppToolkit.Core.Extensions;
using RunnethOverStudio.AppToolkit.Modules.DataAccess.OS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RunnethOverStudio.AppToolkit.Modules.DataAccess.SQL;

/// <summary>
/// Provides methods to ensure the application's database is created and up-to-date.
/// </summary>
public static class SqliteDatabaseInitializer
{
    private static readonly object _initLock = new();

    /// <summary>
    /// Gets the full file path to the application's SQLite database file.
    /// </summary>
    /// <returns>
    /// The absolute path to the database file, constructed from the application's directory and name.
    /// </returns>
    public static string GetDBPath()
    {
        string appFolderPath = AppFileSystem.GetAppDirectoryPath();
        string appName = new DirectoryInfo(appFolderPath).Name;

        return Path.Join(appFolderPath, $"{appName}.db");
    }

    /// <summary>
    /// Ensures the application's database exists and is current by creating it if necessary
    /// and applying any pending migrations.
    /// </summary>
    /// <remarks>
    /// This method is thread-safe and should be called during application initialization 
    /// before configuring data access services.
    /// </remarks>
    public static void InitializeDatabase()
    {
        lock (_initLock)
        {
            string appFolderPath = AppFileSystem.GetAppDirectoryPath();
            string appName = new DirectoryInfo(appFolderPath).Name;
            string dbPath = Path.Join(appFolderPath, $"{appName}.db");

            try
            {
                if (File.Exists(dbPath))
                {
                    UpdateExistingSqliteDB(dbPath);
                }
                else
                {
                    CreateNewSqliteDB(dbPath);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to initialize the database.", ex);
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
                AppliedOn TEXT NOT NULL
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

        foreach (Type assemblyType in Assembly.GetExecutingAssembly().GetTypes())
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

        foreach (BaseMigration migration in migrations.OrderBy(m => m.Number()))
        {
            migration.Run(connection);
        }
    }
}
