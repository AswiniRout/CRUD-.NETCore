namespace StudentCrud.Models;

/// <summary>
/// OOP Concept: INHERITANCE + ENCAPSULATION + POLYMORPHISM.
/// The Student entity INHERITS Id and CreatedAt from <see cref="BaseEntity"/>.
/// Its own data (name, email, age) is ENCAPSULATED behind properties.
/// </summary>
public class Student : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int Age { get; set; }

    /// <summary>
    /// A read-only computed property. Combines the first and last name.
    /// This is ENCAPSULATION: callers get the full name without knowing how it is built.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// OOP Concept: POLYMORPHISM (method overriding).
    /// We override the base object.ToString() to give a friendly description.
    /// </summary>
    public override string ToString() => $"Student #{Id}: {FullName} ({Email}), Age {Age}";
}
