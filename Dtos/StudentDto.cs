using System.ComponentModel.DataAnnotations;

namespace StudentCrud.Dtos;

/// <summary>
/// OOP Concept: ABSTRACTION + ENCAPSULATION.
/// A DTO (Data Transfer Object) is the shape the API accepts from clients.
/// Keeping it separate from the database entity means clients cannot set
/// server-controlled fields like Id or CreatedAt. The validation attributes
/// enforce the rules before the data ever reaches the database.
/// </summary>
public class StudentDto
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]
    public int Age { get; set; }
}
