using System;
using System.Text.Json;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Exceptions;
using static ECommerce.Application.Exceptions.AppException;

namespace ECommerce.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleExceptionAsync(context,ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        if(exception is ValidationException validationEx)
        {
            _logger.LogWarning(
                exception,
                "Validasyon Hatası. TraceId: {TraceId}, Path: {Path}",
                context.TraceIdentifier,
                context.Request.Path
            );
            context.Response.StatusCode=StatusCodes.Status400BadRequest;
            await WriteJsonAsync(
                context,
                ApiResponseFactory.Failure(validationEx.Errors, validationEx.Message)
            );
            return;
        }

        if(exception is AppException appEx)
        {
            LogAppException(context, appEx);
            context.Response.StatusCode = appEx.StatusCode;
            await WriteJsonAsync(context, ApiResponseFactory.Failure(appEx.Message));
            return;
        }

        if(exception is UnauthorizedAccessException)
        {
            _logger.LogWarning(
                exception,
                "Yetkisiz erişim. TraceId: {TraceId}, Path: {Path}",
                context.TraceIdentifier,
                context.Request.Path
            );
            context.Response.StatusCode=StatusCodes.Status401Unauthorized;
            await WriteJsonAsync(
                context,
                ApiResponseFactory.Failure("Bu işlem için yetkiniz yok")
            );
            return;
        }

        if(exception is ArgumentException or ArgumentNullException)
        {
            _logger.LogWarning(
                exception,
                "Geçersiz argüman. TraceId: {TraceId}, Path: {Path}",
                context.TraceIdentifier,
                context.Request.Path
            );
            context.Response.StatusCode=StatusCodes.Status400BadRequest;
            await WriteJsonAsync(
                context,
                ApiResponseFactory.Failure(exception.Message)
            );
            return;
        }

        _logger.LogError(
            exception,
            "İşlenemeyen hata. TraceId: {TraceId}, Path: {Path}",
            context.TraceIdentifier,
            context.Request.Path
        );
        context.Response.StatusCode=StatusCodes.Status500InternalServerError;
        await WriteJsonAsync(
            context,
            ApiResponseFactory.Failure("Beklenmedik bir hata oluştu. Lütfen daha sonra yeniden deneyin.",$"TraceId: {context.TraceIdentifier}")
        );

    }

    private static Task WriteJsonAsync(HttpContext context, ApiResponse<object?> response) => context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));

    private void LogAppException(HttpContext context, AppException appEx)
    {
        if(appEx.StatusCode>=StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                appEx,
                "Uygulama hatası ({StatusCode}). TraceId: {TraceId}, Path: {Path}",
                appEx.StatusCode,
                context.TraceIdentifier,
                context.Request.Path
            );
            return;
        }

            _logger.LogWarning(
                appEx,
                "Uygulama hatası ({StatusCode}). TraceId: {TraceId}, Path: {Path}",
                appEx.StatusCode,
                context.TraceIdentifier,
                context.Request.Path
            );
    }

}
