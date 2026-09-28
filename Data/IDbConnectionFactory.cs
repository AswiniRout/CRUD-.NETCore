using System.Data;

namespace StudentCrud.Data;

/// <summary>
/// OOP Concept: ABSTRACTION.
/// An interface that hides HOW a database connection is created.
/// Any class that needs a connection depends on this abstraction, not on a
/// concrete SQL Server class, which keeps the code loosely coupled and testable.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>Creates and returns a new, ready-to-open database connection.</summary>
    IDbConnection CreateConnection();
}
