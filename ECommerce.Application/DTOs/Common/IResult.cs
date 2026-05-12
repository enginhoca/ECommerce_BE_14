using System;

namespace ECommerce.Application.DTOs.Common;

public interface IResult
{
    bool IsSuccess {get;}
    string? Error {get;}
    ResultErrorType ErrorType {get; }
    object? GetValue();
}
