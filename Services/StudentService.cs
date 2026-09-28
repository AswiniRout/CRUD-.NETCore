using StudentCrud.Dtos;
using StudentCrud.Models;
using StudentCrud.Repositories;

namespace StudentCrud.Services;

/// <summary>
/// OOP Concept: ABSTRACTION + ENCAPSULATION.
/// The service holds the business logic and translates DTOs into entities.
/// It depends on the IStudentRepository ABSTRACTION, so it neither knows nor
/// cares that Dapper and stored procedures are used underneath.
/// </summary>
public class StudentService : IStudentService
{
    private readonly IStudentRepository _repository;

    public StudentService(IStudentRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Student>> GetAllStudentsAsync() => _repository.GetAllAsync();

    public Task<Student?> GetStudentByIdAsync(int id) => _repository.GetByIdAsync(id);

    /// <summary>Maps the incoming DTO to a Student entity, then persists it.</summary>
    public Task<Student> AddStudentAsync(StudentDto dto)
    {
        Student student = MapToEntity(dto);
        return _repository.CreateAsync(student);
    }

    /// <summary>Maps the DTO to an entity, sets the Id, then updates.</summary>
    public Task<bool> UpdateStudentAsync(int id, StudentDto dto)
    {
        Student student = MapToEntity(dto);
        student.Id = id;
        return _repository.UpdateAsync(student);
    }

    public Task<bool> RemoveStudentAsync(int id) => _repository.DeleteAsync(id);

    /// <summary>
    /// A private helper that keeps DTO-to-entity mapping in one place.
    /// Being private is ENCAPSULATION: it is an internal implementation detail.
    /// </summary>
    private static Student MapToEntity(StudentDto dto) => new()
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Email = dto.Email,
        Age = dto.Age
    };
}
