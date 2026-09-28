using Microsoft.AspNetCore.Mvc;
using StudentCrud.Dtos;
using StudentCrud.Models;
using StudentCrud.Services;

namespace StudentCrud.Controllers;

/// <summary>
/// OOP Concept: ABSTRACTION + ENCAPSULATION.
/// The controller only handles HTTP concerns (routes, status codes) and
/// delegates every bit of real work to the IStudentService abstraction.
/// It knows nothing about Dapper, SQL Server or stored procedures.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _service;

    // Dependency injection: the service is provided by the DI container.
    public StudentsController(IStudentService service)
    {
        _service = service;
    }

    /// <summary>GET api/students -> return every student.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Student>>> GetAll()
    {
        IEnumerable<Student> students = await _service.GetAllStudentsAsync();
        return Ok(students);
    }

    /// <summary>GET api/students/{id} -> return one student or 404.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Student>> GetById(int id)
    {
        Student? student = await _service.GetStudentByIdAsync(id);

        if (student is null)
        {
            return NotFound($"No student found with Id {id}.");
        }

        return Ok(student);
    }

    /// <summary>POST api/students -> create a student and return 201 Created.</summary>
    [HttpPost]
    public async Task<ActionResult<Student>> Create([FromBody] StudentDto dto)
    {
        // ModelState is populated automatically from the DTO's validation attributes.
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Student created = await _service.AddStudentAsync(dto);

        // Return 201 with a Location header pointing at the new resource.
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>PUT api/students/{id} -> update a student, or 404 if it does not exist.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] StudentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        bool updated = await _service.UpdateStudentAsync(id, dto);

        if (!updated)
        {
            return NotFound($"No student found with Id {id}.");
        }

        return NoContent();
    }

    /// <summary>DELETE api/students/{id} -> delete a student, or 404 if it does not exist.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        bool deleted = await _service.RemoveStudentAsync(id);

        if (!deleted)
        {
            return NotFound($"No student found with Id {id}.");
        }

        return NoContent();
    }
}
