namespace AiChat.Api.Services;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> _logger)
    {
        this._logger = _logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred.");

        var (statusCode, message) = exception switch
        {
            // تشخیص خطای اختصاصی Ollama که خودت Throw کردی
            Exception ex when ex.Message.Contains("not found, try pulling it first")
                => (StatusCodes.Status404NotFound, "مدل هوش مصنوعی انتخاب شده در سرور یافت نشد. لطفاً مدل دیگری را انتخاب کنید."),

            HttpRequestException => (StatusCodes.Status503ServiceUnavailable, "ارتباط با سرور هوش مصنوعی برقرار نشد."),

            _ => (StatusCodes.Status500InternalServerError, "یک خطای غیرمنتظره در سرور رخ داده است.")
        };

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = "خطا در عملیات",
            Detail = message,
            Instance = httpContext.Request.Path
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
