using StudentCrud.Data;
using StudentCrud.Repositories;
using StudentCrud.Services;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Register services (Dependency Injection) --------------------
// DI is what makes the OOP abstractions work: each class asks for an interface,
// and the container supplies the concrete implementation registered below.

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Read the SQL Server connection string from appsettings.json.
string connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");

// Register the connection factory as a SINGLETON: one instance can safely create
// many short-lived connections, and it only holds an immutable connection string.
builder.Services.AddSingleton<IDbConnectionFactory>(
    _ => new SqlConnectionFactory(connectionString));

// Repository and service are SCOPED: one instance per HTTP request.
// Note we register the INTERFACE mapped to the concrete class (abstraction in action).
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();

var app = builder.Build();

// -------------------- Configure the HTTP request pipeline --------------------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Serve the front-end UI from wwwroot.
// UseDefaultFiles lets "/" fall back to wwwroot/index.html.
// UseStaticFiles serves the html, css and js files.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

app.Run();
