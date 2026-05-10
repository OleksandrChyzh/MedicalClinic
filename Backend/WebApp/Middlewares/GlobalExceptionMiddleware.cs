using System.Net;
using System.Text.Json;

namespace WebApp.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Передаємо запит далі по конвеєру (в контролери, сервіси тощо)
            await _next(context);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException && ex is not StackOverflowException)
        {
            // Ловимо лише безпечні для додатку помилки, логуємо їх
            _logger.LogError(ex, "Виникла помилка під час обробки запиту: {Message}", ex.Message);

            // Формуємо правильну відповідь клієнту
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Вказуємо, що відповідь буде у форматі JSON
        context.Response.ContentType = "application/json";

        // Визначаємо HTTP статус-код залежно від типу винятку
        context.Response.StatusCode = exception switch
        {
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized, // 401
            KeyNotFoundException => (int)HttpStatusCode.NotFound,            // 404
            ArgumentException => (int)HttpStatusCode.BadRequest,             // 400
            InvalidOperationException => (int)HttpStatusCode.BadRequest,     // 400
            _ => (int)HttpStatusCode.InternalServerError                     // 500 (всі інші непередбачувані помилки)
        };

        // Формуємо красивий об'єкт для фронтенду
        var response = new
        {
            statusCode = context.Response.StatusCode,
            message = exception.Message // Можна повертати повідомлення з самого Exception
        };

        var payload = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(payload);
    }
}
