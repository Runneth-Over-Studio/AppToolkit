using Dapper;
using System;
using System.Data.Common;

namespace RunnethOverStudio.AppToolkit.Modules.DataAccess;

/// <summary>
/// Represents the base class for database migrations, including methods for executing migration logic
/// and updating the migration tracking table.
/// </summary>
/// <remarks>
/// This implementation of <see cref="IMigration"/> uses SQLite as the database engine.
/// </remarks>
public abstract class SQLiteMigration : IMigration
{
    /// <inheritdoc/>
    public abstract uint Number { get; init; }

    /// <summary>
    /// A brief, human-readable description of the migration's purpose or changes.
    /// </summary>
    public abstract string Description { get; init; }

    /// <inheritdoc/>
    public void Run(DbConnection dbConnection)
    {
        // Check if this migration number already exists.
        const string checkSql = @"SELECT COUNT(1) FROM Migration WHERE Number = @Number;";
        int exists = dbConnection.ExecuteScalar<int>(checkSql, new { Number = this.Number });
        if (exists > 0)
        {
            throw new InvalidOperationException($"Migration number {this.Number} has already been applied.");
        }

        // Begin transaction, execute migration, update table, commit.
        using DbTransaction transaction = dbConnection.BeginTransaction();
        try
        {
            Execute(dbConnection, transaction);
            UpdateMigrationTable(dbConnection, transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Executes the migration logic.
    /// </summary>
    /// <param name="dbConnection">The database connection to use for the migration.</param>
    /// <param name="transaction">The database transaction within which the migration should be executed.</param>
    protected abstract void Execute(DbConnection dbConnection, DbTransaction transaction);

    /// <summary>
    /// Updates the migration tracking table by inserting a record for this applied migration.
    /// </summary>
    /// <param name="dbConnection">The database connection to use for updating the migration table.</param>
    /// <param name="transaction">The database transaction within which the update should be executed.</param>
    /// <remarks>
    /// This method records the migration's number and description in the <c>Migration</c> table.
    /// </remarks>
    /// <exception cref="System.Data.Common.DbException">
    /// Thrown if the <c>Migration</c> table does not exist or the insert operation fails.
    /// </exception>
    private void UpdateMigrationTable(DbConnection dbConnection, DbTransaction transaction)
    {
        const string sql = @"
            INSERT INTO Migration (Number, Description)
            VALUES (@Number, @Description);
        ";

        dbConnection.Execute(sql, param: new { this.Number, this.Description }, transaction: transaction);
    }

    /// <summary>
    /// Returns the SQL script required to create the <c>Migration</c> tracking table in a SQLite database.
    /// </summary>
    public static string GetMigrationTableCreationScript()
    {
        return @"
            CREATE TABLE IF NOT EXISTS Migration (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CreatedAt TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP)
                Number INTEGER NOT NULL,
                Description TEXT NOT NULL;
            );
        ";
    }
}
