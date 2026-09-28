using System.Data;
using Dapper;
using StudentCrud.Data;
using StudentCrud.Models;

namespace StudentCrud.Repositories;

/// <summary>
/// OOP Concept: ABSTRACTION (implements the interface) + ENCAPSULATION.
/// The concrete data-access class. It uses Dapper to call SQL Server STORED
/// PROCEDURES only, never inline SQL. All database details are ENCAPSULATED here,
/// so the layers above stay clean and technology-agnostic.
/// </summary>
public class StudentRepository : IStudentRepository
{
    // Dependency on the ABSTRACTION, not the concrete SqlConnectionFactory.
    private readonly IDbConnectionFactory _connectionFactory;

    public StudentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>Calls usp_Student_GetAll and maps every row to a Student.</summary>
    public async Task<IEnumerable<Student>> GetAllAsync()
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Student>(
            "usp_Student_GetAll",
            commandType: CommandType.StoredProcedure);
    }

    /// <summary>Calls usp_Student_GetById. Returns null when no student matches.</summary>
    public async Task<Student?> GetByIdAsync(int id)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        // Parameters passed as an anonymous object; Dapper maps them to @Id.
        return await connection.QueryFirstOrDefaultAsync<Student>(
            "usp_Student_GetById",
            new { Id = id },
            commandType: CommandType.StoredProcedure);
    }

    /// <summary>
    /// Calls usp_Student_Create. The procedure returns the inserted row,
    /// including the database-generated Id and CreatedAt, which we return.
    /// </summary>
    public async Task<Student> CreateAsync(Student student)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        Student created = await connection.QuerySingleAsync<Student>(
            "usp_Student_Create",
            new
            {
                student.FirstName,
                student.LastName,
                student.Email,
                student.Age
            },
            commandType: CommandType.StoredProcedure);

        return created;
    }

    /// <summary>
    /// Calls usp_Student_Update. The procedure returns AffectedRows,
    /// so we can tell the caller whether the student actually existed.
    /// </summary>
    public async Task<bool> UpdateAsync(Student student)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        int affectedRows = await connection.ExecuteScalarAsync<int>(
            "usp_Student_Update",
            new
            {
                student.Id,
                student.FirstName,
                student.LastName,
                student.Email,
                student.Age
            },
            commandType: CommandType.StoredProcedure);

        return affectedRows > 0;
    }

    /// <summary>Calls usp_Student_Delete. Returns true when a row was removed.</summary>
    public async Task<bool> DeleteAsync(int id)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        int affectedRows = await connection.ExecuteScalarAsync<int>(
            "usp_Student_Delete",
            new { Id = id },
            commandType: CommandType.StoredProcedure);

        return affectedRows > 0;
    }
}
