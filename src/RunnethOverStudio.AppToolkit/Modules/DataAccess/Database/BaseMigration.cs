using Dapper;
using System.Data.Common;

namespace RunnethOverStudio.AppToolkit.Modules.DataAccess;

/// <summary>
/// Represents the base class for all database migrations.
/// Provides a template for executing migration logic and updating the migration tracking table.
/// </summary>
/// <remarks>
/// When creating tables in SQLite, define primary key columns with <c>INTEGER PRIMARY KEY AUTOINCREMENT</c> 
/// so that the column is made to be an alias for the internal ROWID.
/// </remarks>
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
    /// <exception cref="DbException">
    /// Thrown if the <c>Migration</c> table does not exist or does not contain a <c>Number</c> column.
    /// </exception>
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
            INSERT INTO Migration (Number)
            VALUES (@Number);
        ";

        dbConnection.Execute(sql, param: new
        {
            Number = Number()
        });
    }
}
