using System;

namespace ECommerce.Application.DTOs.Common;

public class ApiResponse<T> : IApiResponse
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public IReadOnlyList<string> Errors { get; set; } = [];
    public string? Message { get; init; }
}
