/* ============================================================================
   Student CRUD - Database setup script
   Run this script in SQL Server Management Studio (SSMS) or Azure Data Studio
   BEFORE running the API. It creates the database, the table and all the
   stored procedures the app uses (the app never sends raw SQL, only SP calls).
   ============================================================================ */

-- 1) Create the database if it does not already exist
IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'CRUD')
BEGIN
    CREATE DATABASE CRUD;
END
GO

USE CRUD;
GO

-- 2) Create the Students table if it does not already exist
IF OBJECT_ID(N'dbo.Students', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Students
    (
        Id          INT IDENTITY(1,1) PRIMARY KEY,   -- Auto-increment primary key
        FirstName   NVARCHAR(100) NOT NULL,
        LastName    NVARCHAR(100) NOT NULL,
        Email       NVARCHAR(200) NOT NULL,
        Age         INT           NOT NULL,
        CreatedAt   DATETIME2     NOT NULL DEFAULT (SYSUTCDATETIME())
    );
END
GO

/* ----------------------------------------------------------------------------
   STORED PROCEDURE: Create a new student
   Returns the newly created row (so the API can echo back the generated Id).
   ---------------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_Student_Create
    @FirstName NVARCHAR(100),
    @LastName  NVARCHAR(100),
    @Email     NVARCHAR(200),
    @Age       INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Students (FirstName, LastName, Email, Age)
    VALUES (@FirstName, @LastName, @Email, @Age);

    -- Return the row we just inserted using the new identity value
    SELECT Id, FirstName, LastName, Email, Age, CreatedAt
    FROM dbo.Students
    WHERE Id = SCOPE_IDENTITY();
END
GO

/* ----------------------------------------------------------------------------
   STORED PROCEDURE: Get all students
   ---------------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_Student_GetAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, FirstName, LastName, Email, Age, CreatedAt
    FROM dbo.Students
    ORDER BY Id;
END
GO

/* ----------------------------------------------------------------------------
   STORED PROCEDURE: Get a single student by Id
   ---------------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_Student_GetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, FirstName, LastName, Email, Age, CreatedAt
    FROM dbo.Students
    WHERE Id = @Id;
END
GO

/* ----------------------------------------------------------------------------
   STORED PROCEDURE: Update an existing student
   Returns the number of affected rows (0 means the Id was not found).
   ---------------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_Student_Update
    @Id        INT,
    @FirstName NVARCHAR(100),
    @LastName  NVARCHAR(100),
    @Email     NVARCHAR(200),
    @Age       INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Students
    SET FirstName = @FirstName,
        LastName  = @LastName,
        Email     = @Email,
        Age       = @Age
    WHERE Id = @Id;

    SELECT @@ROWCOUNT AS AffectedRows;
END
GO

/* ----------------------------------------------------------------------------
   STORED PROCEDURE: Delete a student by Id
   Returns the number of affected rows (0 means the Id was not found).
   ---------------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.usp_Student_Delete
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Students
    WHERE Id = @Id;

    SELECT @@ROWCOUNT AS AffectedRows;
END
GO
