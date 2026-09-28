# Student CRUD API - Step by Step Build Guide

A hands-on guide to build a simple **Student CRUD Web API** using **.NET Core**, **Dapper**, and **SQL Server stored procedures**, structured around the four **OOP** pillars.

This guide takes you from an empty folder to a running API.

---

## What you will build

A REST API that manages students with five operations:

| Operation | HTTP Verb | Route |
| --- | --- | --- |
| Get all students | GET | `/api/students` |
| Get one student | GET | `/api/students/{id}` |
| Create a student | POST | `/api/students` |
| Update a student | PUT | `/api/students/{id}` |
| Delete a student | DELETE | `/api/students/{id}` |

Data access uses **Dapper** and calls **stored procedures only** (no inline SQL).

---

## Prerequisites

Install these before you start:

- **.NET SDK 8.0 or later** - check with `dotnet --version`
- **SQL Server** or **SQL Server LocalDB** (LocalDB ships with Visual Studio)
- **SSMS** or **Azure Data Studio** to run the SQL script
- A code editor: **Visual Studio**, **VS Code**, or **Kiro**

---

## Project structure (final result)

```
CRUD/
├── Controllers/
│   └── StudentsController.cs     # HTTP endpoints
├── Data/
│   ├── IDbConnectionFactory.cs   # connection abstraction
│   └── SqlConnectionFactory.cs   # SQL Server implementation
├── Database/
│   └── StudentDb.sql             # table + stored procedures
├── Dtos/
│   └── StudentDto.cs             # request model with validation
├── Models/
│   ├── BaseEntity.cs             # abstract base class
│   └── Student.cs                # student entity
├── Repositories/
│   ├── IStudentRepository.cs     # data-access contract
│   └── StudentRepository.cs      # Dapper implementation
├── Services/
│   ├── IStudentService.cs        # business-logic contract
│   └── StudentService.cs         # business-logic implementation
├── appsettings.json              # connection string
└── Program.cs                    # DI wiring + pipeline
```

**Why so many folders?** Each layer has one job. The controller handles HTTP, the service holds logic, the repository talks to the database. Layers depend on **interfaces**, not concrete classes, so any piece can be swapped without touching the others.

---

## The OOP concepts, mapped to files

| OOP Pillar | Where you see it |
| --- | --- |
| **Abstraction** | Every layer depends on an interface (`IDbConnectionFactory`, `IStudentRepository`, `IStudentService`) |
| **Encapsulation** | Connection string, SQL details, and DTO-to-entity mapping are hidden inside their classes |
| **Inheritance** | `Student` inherits `Id` and `CreatedAt` from the abstract `BaseEntity` |
| **Polymorphism** | `Student` overrides `ToString()`; DI supplies concrete types wherever an interface is requested |

---

## Step 1 - Create the project

Open a terminal in the folder where you want the project, then run:

```bash
dotnet new webapi --use-controllers -n StudentCrud -o .
```

- `--use-controllers` gives you a controller-based API (not minimal APIs).
- `-o .` creates it in the current folder.

Delete the two sample files the template adds:

```bash
# Windows PowerShell
Remove-Item WeatherForecast.cs
Remove-Item Controllers/WeatherForecastController.cs
```

---

## Step 2 - Add the NuGet packages

```bash
dotnet add package Dapper
dotnet add package Microsoft.Data.SqlClient
```

- **Dapper** is a lightweight micro-ORM that maps query results to objects.
- **Microsoft.Data.SqlClient** is the SQL Server database driver.

Your `.csproj` should now list both packages under `<ItemGroup>`.

---

## Step 3 - Create the database and stored procedures

Create `Database/StudentDb.sql` and run it in SSMS or Azure Data Studio.

It does four things:
1. Creates the `CRUD` database (if missing).
2. Creates the `Students` table.
3. Creates five stored procedures: create, get all, get by id, update, delete.
4. The create procedure returns the inserted row; update and delete return the affected row count.

```sql
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'CRUD')
BEGIN
    CREATE DATABASE CRUD;
END
GO

USE CRUD;
GO

IF OBJECT_ID(N'dbo.Students', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Students
    (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        FirstName   NVARCHAR(100) NOT NULL,
        LastName    NVARCHAR(100) NOT NULL,
        Email       NVARCHAR(200) NOT NULL,
        Age         INT           NOT NULL,
        CreatedAt   DATETIME2     NOT NULL DEFAULT (SYSUTCDATETIME())
    );
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Student_Create
    @FirstName NVARCHAR(100), @LastName NVARCHAR(100),
    @Email NVARCHAR(200), @Age INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Students (FirstName, LastName, Email, Age)
    VALUES (@FirstName, @LastName, @Email, @Age);
    SELECT Id, FirstName, LastName, Email, Age, CreatedAt
    FROM dbo.Students WHERE Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Student_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, FirstName, LastName, Email, Age, CreatedAt
    FROM dbo.Students ORDER BY Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Student_GetById @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, FirstName, LastName, Email, Age, CreatedAt
    FROM dbo.Students WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Student_Update
    @Id INT, @FirstName NVARCHAR(100), @LastName NVARCHAR(100),
    @Email NVARCHAR(200), @Age INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Students
    SET FirstName = @FirstName, LastName = @LastName,
        Email = @Email, Age = @Age
    WHERE Id = @Id;
    SELECT @@ROWCOUNT AS AffectedRows;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Student_Delete @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Students WHERE Id = @Id;
    SELECT @@ROWCOUNT AS AffectedRows;
END
GO
```

> **Tip:** `CREATE OR ALTER` means you can re-run the script safely while developing.

---

## Step 4 - Build the Model layer (Inheritance + Encapsulation)

**`Models/BaseEntity.cs`** - an abstract base class holding fields every entity shares:

```csharp
namespace StudentCrud.Models;

// Abstract: only exists to be inherited, never created on its own.
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**`Models/Student.cs`** - inherits the base and adds its own data:

```csharp
namespace StudentCrud.Models;

public class Student : BaseEntity   // Inheritance
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }

    // Computed, read-only property (Encapsulation)
    public string FullName => $"{FirstName} {LastName}";

    // Override base method (Polymorphism)
    public override string ToString() => $"Student #{Id}: {FullName} ({Email}), Age {Age}";
}
```

---

## Step 5 - Build the DTO (Abstraction + validation)

**`Dtos/StudentDto.cs`** - the shape clients send in. It hides server-owned fields (`Id`, `CreatedAt`) and validates input:

```csharp
using System.ComponentModel.DataAnnotations;

namespace StudentCrud.Dtos;

public class StudentDto
{
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]
    public int Age { get; set; }
}
```

---

## Step 6 - Build the connection factory (Abstraction + Encapsulation)

**`Data/IDbConnectionFactory.cs`**:

```csharp
using System.Data;

namespace StudentCrud.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
```

**`Data/SqlConnectionFactory.cs`** - hides the connection string behind the interface:

```csharp
using System.Data;
using Microsoft.Data.SqlClient;

namespace StudentCrud.Data;

public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;   // hidden detail

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
```

---

## Step 7 - Build the Repository (Dapper + stored procedures)

**`Repositories/IStudentRepository.cs`** - the CRUD contract:

```csharp
using StudentCrud.Models;

namespace StudentCrud.Repositories;

public interface IStudentRepository
{
    Task<IEnumerable<Student>> GetAllAsync();
    Task<Student?> GetByIdAsync(int id);
    Task<Student> CreateAsync(Student student);
    Task<bool> UpdateAsync(Student student);
    Task<bool> DeleteAsync(int id);
}
```

**`Repositories/StudentRepository.cs`** - calls the stored procedures with Dapper. Note `CommandType.StoredProcedure` on every call:

```csharp
using System.Data;
using Dapper;
using StudentCrud.Data;
using StudentCrud.Models;

namespace StudentCrud.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public StudentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Student>> GetAllAsync()
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Student>(
            "usp_Student_GetAll", commandType: CommandType.StoredProcedure);
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Student>(
            "usp_Student_GetById", new { Id = id },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Student> CreateAsync(Student student)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<Student>(
            "usp_Student_Create",
            new { student.FirstName, student.LastName, student.Email, student.Age },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> UpdateAsync(Student student)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();
        int affected = await connection.ExecuteScalarAsync<int>(
            "usp_Student_Update",
            new { student.Id, student.FirstName, student.LastName, student.Email, student.Age },
            commandType: CommandType.StoredProcedure);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();
        int affected = await connection.ExecuteScalarAsync<int>(
            "usp_Student_Delete", new { Id = id },
            commandType: CommandType.StoredProcedure);
        return affected > 0;
    }
}
```

> **Why `using`?** It closes the database connection automatically when the method ends, even if an error occurs.

---

## Step 8 - Build the Service layer (business logic)

**`Services/IStudentService.cs`**:

```csharp
using StudentCrud.Dtos;
using StudentCrud.Models;

namespace StudentCrud.Services;

public interface IStudentService
{
    Task<IEnumerable<Student>> GetAllStudentsAsync();
    Task<Student?> GetStudentByIdAsync(int id);
    Task<Student> AddStudentAsync(StudentDto dto);
    Task<bool> UpdateStudentAsync(int id, StudentDto dto);
    Task<bool> RemoveStudentAsync(int id);
}
```

**`Services/StudentService.cs`** - turns DTOs into entities and delegates to the repository:

```csharp
using StudentCrud.Dtos;
using StudentCrud.Models;
using StudentCrud.Repositories;

namespace StudentCrud.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _repository;

    public StudentService(IStudentRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Student>> GetAllStudentsAsync() => _repository.GetAllAsync();

    public Task<Student?> GetStudentByIdAsync(int id) => _repository.GetByIdAsync(id);

    public Task<Student> AddStudentAsync(StudentDto dto)
        => _repository.CreateAsync(MapToEntity(dto));

    public Task<bool> UpdateStudentAsync(int id, StudentDto dto)
    {
        Student student = MapToEntity(dto);
        student.Id = id;
        return _repository.UpdateAsync(student);
    }

    public Task<bool> RemoveStudentAsync(int id) => _repository.DeleteAsync(id);

    // private helper = internal detail (Encapsulation)
    private static Student MapToEntity(StudentDto dto) => new()
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Email = dto.Email,
        Age = dto.Age
    };
}
```

---

## Step 9 - Build the Controller (HTTP endpoints)

**`Controllers/StudentsController.cs`** - handles only HTTP concerns and delegates the rest:

```csharp
using Microsoft.AspNetCore.Mvc;
using StudentCrud.Dtos;
using StudentCrud.Models;
using StudentCrud.Services;

namespace StudentCrud.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _service;

    public StudentsController(IStudentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Student>>> GetAll()
        => Ok(await _service.GetAllStudentsAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Student>> GetById(int id)
    {
        Student? student = await _service.GetStudentByIdAsync(id);
        return student is null ? NotFound($"No student found with Id {id}.") : Ok(student);
    }

    [HttpPost]
    public async Task<ActionResult<Student>> Create([FromBody] StudentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        Student created = await _service.AddStudentAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] StudentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        bool updated = await _service.UpdateStudentAsync(id, dto);
        return updated ? NoContent() : NotFound($"No student found with Id {id}.");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        bool deleted = await _service.RemoveStudentAsync(id);
        return deleted ? NoContent() : NotFound($"No student found with Id {id}.");
    }
}
```

---

## Step 10 - Wire up Dependency Injection

**`Program.cs`** - register every interface with its concrete class:

```csharp
using StudentCrud.Data;
using StudentCrud.Repositories;
using StudentCrud.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

string connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

// Singleton: only holds an immutable connection string
builder.Services.AddSingleton<IDbConnectionFactory>(
    _ => new SqlConnectionFactory(connectionString));

// Scoped: one instance per HTTP request
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

**Service lifetimes explained:**

| Lifetime | Meaning | Used for |
| --- | --- | --- |
| `Singleton` | one instance for the whole app | connection factory |
| `Scoped` | one instance per request | repository, service |
| `Transient` | a new instance every time | (not used here) |

---

## Step 11 - Configure the connection string

**`appsettings.json`** - add a `ConnectionStrings` section:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=CRUD;Integrated Security=True;Encrypt=True;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> **Note:** In a connection string inside JSON, the backslash must be escaped as `\\`.
> `TrustServerCertificate=True` is needed for LocalDB's self-signed certificate when `Encrypt=True`.

---

## Step 12 - Build and run

```bash
dotnet build
dotnet run
```

The console prints the URL, for example `https://localhost:7xxx`.
In development you can open the OpenAPI document at `/openapi/v1.json`.

---

## Step 13 - Test the endpoints

**Create a student (POST):**

```bash
curl -X POST https://localhost:7xxx/api/students ^
  -H "Content-Type: application/json" ^
  -d "{ \"firstName\": \"Ada\", \"lastName\": \"Lovelace\", \"email\": \"ada@example.com\", \"age\": 28 }"
```

**Get all students (GET):**

```bash
curl https://localhost:7xxx/api/students
```

**Get one student (GET):**

```bash
curl https://localhost:7xxx/api/students/1
```

**Update a student (PUT):**

```bash
curl -X PUT https://localhost:7xxx/api/students/1 ^
  -H "Content-Type: application/json" ^
  -d "{ \"firstName\": \"Ada\", \"lastName\": \"King\", \"email\": \"ada@example.com\", \"age\": 29 }"
```

**Delete a student (DELETE):**

```bash
curl -X DELETE https://localhost:7xxx/api/students/1
```

> The `^` is the line-continuation character in Windows CMD. In PowerShell use a backtick `` ` ``, and in bash use a backslash `\`.

---

## Request flow (how a call travels through the layers)

```
HTTP Request
    |
    v
StudentsController      (HTTP: routes, status codes, validation)
    |
    v
IStudentService  -->  StudentService     (business logic, DTO -> entity)
    |
    v
IStudentRepository  -->  StudentRepository   (Dapper + stored procedures)
    |
    v
IDbConnectionFactory  -->  SqlConnectionFactory   (opens the connection)
    |
    v
SQL Server  (usp_Student_* stored procedures)
```

Each arrow crosses an **interface boundary**. That is abstraction at work: a layer only knows the contract of the layer below it, never the concrete class.

---

## Common issues and fixes

| Problem | Likely cause | Fix |
| --- | --- | --- |
| `Connection string 'DefaultConnection' is missing` | Typo or missing section in `appsettings.json` | Check the `ConnectionStrings` section name and key |
| `Cannot open database "CRUD"` | The SQL script was not run | Run `Database/StudentDb.sql` first |
| `Could not find stored procedure 'usp_Student_...'` | Script ran against the wrong database | Confirm you executed it under the `CRUD` database |
| Certificate / SSL error on connect | `Encrypt=True` with an untrusted cert | Set `TrustServerCertificate=True` |
| `A network-related ... error` | Wrong `Data Source` | Verify the instance name (e.g. `(localdb)\MSSQLLocalDB`) |

---

## Extend it further

Once the basics work, try adding:

- **Global exception handling** with middleware for clean error responses.
- **Logging** on each service method to trace calls.
- **Paging** on `GetAll` (add `@PageNumber` and `@PageSize` to the stored procedure).
- **Unit tests** that mock `IStudentRepository` to test the service in isolation.
- **AutoMapper** to replace the manual `MapToEntity` helper.

---

Built with .NET Core, Dapper, and SQL Server stored procedures.
