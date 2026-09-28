using StudentCrud.Models;

namespace StudentCrud.Repositories;

/// <summary>
/// OOP Concept: ABSTRACTION.
/// This interface defines WHAT can be done with students (the CRUD contract)
/// without saying HOW. The service layer depends on this contract, so the
/// data-access technology (Dapper today, something else tomorrow) can change
/// without touching the rest of the app.
/// </summary>
public interface IStudentRepository
{
    Task<IEnumerable<Student>> GetAllAsync();

    Task<Student?> GetByIdAsync(int id);

    Task<Student> CreateAsync(Student student);

    Task<bool> UpdateAsync(Student student);

    Task<bool> DeleteAsync(int id);
}
