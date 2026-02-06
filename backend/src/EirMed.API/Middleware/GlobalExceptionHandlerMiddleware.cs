using System.Diagnostics;
using System.Net;
using System.Text.Json;
using EirMed.API.Models;
using EirMed.Domain.Exceptions;

namespace EirMed.API.Middleware;

public sealed class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "Recurso não encontrado"),
            Domain.Exceptions.ValidationException => (HttpStatusCode.UnprocessableEntity, "Erro de validação"),
            ConflictException => (HttpStatusCode.Conflict, "Conflito"),
            UnauthorizedException => (HttpStatusCode.Unauthorized, "Não autorizado"),
            ForbiddenException => (HttpStatusCode.Forbidden, "Acesso proibido"),
            _ => (HttpStatusCode.InternalServerError, "Erro interno do servidor")
        };

        LogException(exception, statusCode);

        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        var response = new ErrorResponse
        {
            Status = (int)statusCode,
            Title = title,
            Detail = statusCode == HttpStatusCode.InternalServerError
                ? "Ocorreu um erro inesperado. Tente novamente mais tarde."
                : exception.Message,
            TraceId = traceId,
            Errors = exception is Domain.Exceptions.ValidationException validationException
                ? validationException.Errors
                : null
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, JsonOptions));
    }

    private void LogException(Exception exception, HttpStatusCode statusCode)
    {
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Erro não tratado: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Exceção tratada ({StatusCode}): {Message}",
                (int)statusCode, exception.Message);
        }
    }
}
