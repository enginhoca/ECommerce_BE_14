using System;

namespace ECommerce.Application.DTOs.Common;

public class Result<T> : IResult
{
    public bool IsSuccess { get; private init; }

    public string? Error { get; private init; }

    public ResultErrorType ErrorType { get; private init; }
    public T? Value { get; private init; }

    public object? GetValue() => Value;

    private Result()
    {

    }

    public static Result<T> Success(T value)
    {
        return new Result<T>
        {
            IsSuccess = true,
            Value = value,
            ErrorType = ResultErrorType.None
        };
    }

    public static Result<T> NotFound(string error = "Kaynak bulunamadı!")
    {
        return new Result<T>
        {
            IsSuccess = false,
            Error = error,
            ErrorType = ResultErrorType.NotFound
        };
    }

    public static Result<T> Conflict(string error)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Error = error,
            ErrorType = ResultErrorType.Conflict
        };
    }

    public static Result<T> ValidationFailure(string error)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Error = error,
            ErrorType = ResultErrorType.Validation
        };
    }

    public static Result<T> Forbidden(string error = "Bu işlem için yetkiniz yok!")
    {
        return new Result<T>
        {
            IsSuccess = false,
            Error = error,
            ErrorType = ResultErrorType.Forbidden
        };
    }

    public static Result<T> Unauthorized(string error = "Giriş yapılmamış!")
    {
        return new Result<T>
        {
            IsSuccess = false,
            Error = error,
            ErrorType = ResultErrorType.Unauthorized
        };
    }

    public static Result<T> Unexpected(string error = "Beklenmeyen bir hata oluştu!")
    {
        return new Result<T>
        {
            IsSuccess = false,
            Error = error,
            ErrorType = ResultErrorType.Unexpected
        };
    }

}
