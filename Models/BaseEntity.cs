namespace StudentCrud.Models;

/// <summary>
/// OOP Concept: ABSTRACTION + INHERITANCE.
/// A base class that holds the fields every database entity shares.
/// It is declared "abstract" because a BaseEntity on its own has no meaning,
/// it only exists to be inherited by concrete entities such as <see cref="Student"/>.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Primary key. Common to every entity, so it lives in the base class.</summary>
    public int Id { get; set; }

    /// <summary>The UTC time the row was created. Also common to every entity.</summary>
    public DateTime CreatedAt { get; set; }
}
