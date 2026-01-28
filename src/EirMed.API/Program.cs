using EirMed.API.Middleware;
using EirMed.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseGlobalExceptionHandler();
app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.MapGet("/protected-test", (System.Security.Claims.ClaimsPrincipal user) =>
{
    var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? user.FindFirst("sub")?.Value;
    var email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
        ?? user.FindFirst("email")?.Value;
    var name = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
        ?? user.FindFirst("name")?.Value;

    return Results.Ok(new
    {
        message = "Acesso autorizado!",
        userId,
        email,
        name,
        timestamp = DateTime.UtcNow
    });
}).RequireAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/error-test/not-found", () =>
    {
        throw new EirMed.Domain.Exceptions.NotFoundException("Paciente", 999);
    });

    app.MapGet("/error-test/validation", () =>
    {
        throw new EirMed.Domain.Exceptions.ValidationException(
            new Dictionary<string, string[]>
            {
                { "Nome", ["O campo Nome é obrigatório."] },
                { "Email", ["O campo Email deve ser um endereço de e-mail válido."] }
            });
    });

    app.MapGet("/error-test/conflict", () =>
    {
        throw new EirMed.Domain.Exceptions.ConflictException("Já existe um registro com esses dados.");
    });

    app.MapGet("/error-test/unauthorized", () =>
    {
        throw new EirMed.Domain.Exceptions.UnauthorizedException();
    });

    app.MapGet("/error-test/forbidden", () =>
    {
        throw new EirMed.Domain.Exceptions.ForbiddenException();
    });

    app.MapGet("/error-test/internal", () =>
    {
        throw new InvalidOperationException("Erro inesperado de teste.");
    });
}

app.Run();
