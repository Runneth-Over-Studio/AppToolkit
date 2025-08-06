using Dapper;
using System;
using System.Data.Common;

namespace RunnethOverStudio.AppToolkit.Modules.DataAccess.SQL;

/// <summary>
/// Represents the base class for all database migrations.
/// Provides a template for executing migration logic and updating the migration tracking table.
/// </summary>
public abstract class BaseMigration
{
    /// <summary>
    /// Gets the unique migration number.
    /// </summary>
    public abstract uint Number();

    /// <summary>
    /// Executes the migration and updates the migration tracking table.
    /// </summary>
    /// <param name="dbConnection">The database connection to use for the migration.</param>
    public void Run(DbConnection dbConnection)
    {
        Execute(dbConnection);
        UpdateMigrationTable(dbConnection);
    }

    /// <summary>
    /// Executes the migration logic.
    /// </summary>
    /// <param name="dbConnection">The database connection to use for the migration.</param>
    protected abstract void Execute(DbConnection dbConnection);

    private void UpdateMigrationTable(DbConnection dbConnection)
    {
        const string sql = @"
            INSERT INTO Migration (Number, AppliedOn)
            VALUES (@Number, @AppliedOn);
        ";

        dbConnection.Execute(sql, param: new
        {
            Number = this.Number(),
            AppliedOn = DateTime.UtcNow.ToString("o") // ISO 8601 format
        });
    }
}
