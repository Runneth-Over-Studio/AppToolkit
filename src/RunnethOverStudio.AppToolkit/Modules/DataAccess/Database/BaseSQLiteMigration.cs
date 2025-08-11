using Dapper;
using System;
using System.Data.Common;

namespace RunnethOverStudio.AppToolkit.Modules.DataAccess;

/// <summary>
/// Represents the base class for all SQLite database migrations.
/// Provides a template for executing migration logic and updating the migration tracking table.
/// </summary>
/// <remarks>
/// When creating tables in SQLite, define primary key columns with <c>INTEGER PRIMARY KEY AUTOINCREMENT</c> 
/// so that the column is made to be an alias for the internal ROWID.
/// </remarks>
public abstract class BaseSQLiteMigration
{
    /// <summary>
    /// The unique migration number.
    /// </summary>
    public abstract uint Number { get; init; }

    /// <summary>
    /// A brief, human-readable description of the migration's purpose or changes.
    /// </summary>
    public abstract string Description { get; init; }

    /// <summary>
    /// Executes the migration and updates the migration tracking table.
    /// </summary>
    /// <param name="dbConnection">The database connection to use for the migration.</param>
    /// <exception cref="DbException">
    /// Thrown if the <c>Migration</c> table does not exist or does not contain a <c>Number</c> column.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a migration with the same number already exists in the <c>Migration</c> table.
    /// </exception>
    public void Run(DbConnection dbConnection)
    {
        // Check if this migration number already exists.
        const string checkSql = @"SELECT COUNT(1) FROM Migration WHERE Number = @Number;";
        int exists = dbConnection.ExecuteScalar<int>(checkSql, new { Number = this.Number });
        if (exists > 0)
        {
            throw new InvalidOperationException($"Migration number {this.Number} has already been applied.");
        }

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

    private void UpdateMigrationTable(DbConnection dbConnection, DbTransaction transaction)
    {
        const string sql = @"
            INSERT INTO Migration (Number, Description)
            VALUES (@Number, @Description);
        ";

        dbConnection.Execute(sql, param: new { this.Number, this.Description }, transaction: transaction);
    }
}
