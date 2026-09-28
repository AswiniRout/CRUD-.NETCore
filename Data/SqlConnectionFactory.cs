using System.Data;
using Microsoft.Data.SqlClient;

namespace StudentCrud.Data;

/// <summary>
/// OOP Concept: ABSTRACTION (implementation) + ENCAPSULATION.
/// The concrete SQL Server implementation of <see cref="IDbConnectionFactory"/>.
/// It ENCAPSULATES the connection string: no other class needs to know it.
/// </summary>
public class SqlConnectionFactory : IDbConnectionFactory
{
    // Private, read-only field: the connection string is hidden from the outside world.
    private readonly string _connectionString;

    /// <summary>
    /// The connection string is injected via the constructor (dependency injection),
    /// so this class never reaches out to configuration on its own.
    /// </summary>
    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>Creates a brand new SQL Server connection each time it is called.</summary>
    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
