using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Laqeeta.Infrastructure.Data;

public static class DbErrors
{
    /// <summary>
    /// True only when a save failed because a UNIQUE index / constraint was violated (two requests inserted the same
    /// thing). Any other DbUpdateException (timeout, deadlock, NOT NULL, foreign key, ...) is a different problem and must
    /// not be reported as "already exists".
    /// </summary>
    public static bool IsUniqueViolation(DbUpdateException ex)
    {
        var inner = ex.GetBaseException();
        if (inner is SqlException sql)
            return sql.Number is 2601 or 2627;   // SQL Server: duplicate key in a unique index / unique or primary key constraint
        // SQLite (used by the tests): "SQLite Error 19: 'UNIQUE constraint failed: table.column'"
        return inner.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase);
    }
}
