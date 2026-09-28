using StudentCrud.Dtos;
using StudentCrud.Models;

namespace StudentCrud.Services;

/// <summary>
/// OOP Concept: ABSTRACTION.
/// The service contract sits between the controller and the repository.
/// It works with DTOs on the way in and entities on the way out, so the
/// controller never talks to the database layer directly.
/// </summary>
public interface IStudentService
{
    Task<IEnumerable<Student>> GetAllStudentsAsync();

    Task<Student?> GetStudentByIdAsync(int id);

    Task<Student> AddStudentAsync(StudentDto dto);

    Task<bool> UpdateStudentAsync(int id, StudentDto dto);

    Task<bool> RemoveStudentAsync(int id);
}
