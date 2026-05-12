using System;

namespace ECommerce.Application.Exceptions;

public abstract class AppException : Exception
{
    public int StatusCode { get; }

    protected AppException(int statusCode, string message, Exception? innerException = null) : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public sealed class NotFoundException : AppException
    {
        public NotFoundException(string message ) : base(404, message)
        {
        }
    }

    public sealed class ValidationException : AppException
    {
        public IReadOnlyList<string> Errors { get; }
        public ValidationException(IEnumerable<string> errors, string? message = null) : base(400, message ?? "Validasyon hatası.")
        {
            Errors = errors.ToList();
        }

        public ValidationException(string error, string? message = null) : base(400, message ?? "Validasyon hatası.")
        {
            Errors = [error];
        }
    }

    public sealed class BadRequestException : AppException
    {
        public BadRequestException(string message) : base(400, message)
        {
        }
    }

    public sealed class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message = "Kimlik doğrulama gereklidir!") : base(401, message)
        {
        }
    }

    public sealed class ForbiddenException : AppException
    {
        public ForbiddenException(string message = "Bu işlem için yetkiniz yok!") : base(403, message)
        {
        }
    }

    public sealed class ConflictException : AppException
    {
        public ConflictException(string message) : base(409, message)
        {
        }
    }
}
