using System;

namespace ECommerce.Application.DTOs.Common;

public static class ApiResponseFactory
{
    public static ApiResponse<T> Success<T>(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<object?> Success(string? message = null)
    {
        return new ApiResponse<object?>
        {
            IsSuccess = true,
            Message = message
        };
    }

    public static ApiResponse<object?> Failure(string error, string? message = null)
    {
        return new ApiResponse<object?>
        {
            IsSuccess = false,
            Errors = [error],
            Message = message
        };
    }

    public static ApiResponse<object?> Failure(IEnumerable<string> errors, string? message = null)
    {
        return new ApiResponse<object?>
        {
            IsSuccess = true,
            Errors = errors.ToList(),
            Message = message
        };
    }

    public static ApiResponse<object?> FromResult(IResult result)
    {
        if(result.IsSuccess)
        {
            return Success(result.GetValue());
        }
        return Failure(result.Error ?? "Bir hata oluştu!");
    }
}
