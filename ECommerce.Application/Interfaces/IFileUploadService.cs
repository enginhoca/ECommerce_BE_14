using System;
using ECommerce.Application.DTOs.Common;

namespace ECommerce.Application.Interfaces;

public interface IFileUploadService
{
    Task<Result<string>> UploadImageAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken);

    Task<Result<bool>> DeleteImageAsync(string imageUrl, CancellationToken cancellationToken);
}
