using System;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Services;

public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileUploadService> _logger;

    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 5242880;
    public FileUploadService(IWebHostEnvironment env, ILogger<FileUploadService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<Result<bool>> DeleteImageAsync(string imageUrl, CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(imageUrl))
        {
            return Result<bool>.ValidationFailure("Resim boş olamaz!");
        }

        try
        {
            var relativePath = imageUrl.TrimStart('/').Replace('/',Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_env.WebRootPath, relativePath);

            if(File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation($"Dosya silindi: {fullPath}");
            }
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Dosya silinirken hata: {imageUrl}");
            return Result<bool>.Unexpected("Dosya silinirken bir hata oluştu.");
        }
    }

    public async Task<Result<string>> UploadImageAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken)
    {
        // Content Type
        if(!AllowedContentTypes.Contains(contentType.ToLower()))
        {
            return Result<string>.ValidationFailure("Sadece JPEG, PNG, WebP formatı desteklenmektedir.");
        }

        // Uzantı kontrolü
        var extension = Path.GetExtension(fileName).ToLower(); // .png  .jpg
        if(!AllowedExtensions.Contains(extension))
        {
            return Result<string>.ValidationFailure("Geçersiz dosya uzantısı!");
        }

        // Boyut kontrolü
        if(fileStream.Length > MaxFileSizeBytes)
        {
            return Result<string>.ValidationFailure($"Dosya boyutu {MaxFileSizeBytes / 1024 / 1024} MB'ı geçemez!");
        }

        try
        {
            // c:/applications/yowaacademycom/webapi/wwwroot/uploads/products
            var uploadFolder = Path.Combine(_env.WebRootPath,"uploads","products");
            Directory.CreateDirectory(uploadFolder);
            // 1e4cb4af-e26e-4838-a04c-7ab3612553f5.png
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            //  c:/applications/yowaacademycom/webapi/wwwroot/uploads/products/1e4cb4af-e26e-4838-a04c-7ab3612553f5.png

            var filePath = Path.Combine(uploadFolder, uniqueFileName);
            await using var fileOut = new FileStream(filePath,FileMode.Create);
            await fileStream.CopyToAsync(fileOut, cancellationToken);

            var relativeUrl = $"/uploads/products{uniqueFileName}";
            _logger.LogInformation($"Dosya yüklenedi: {relativeUrl}");

            return Result<string>.Success(relativeUrl);            
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dosya yüklenirken bir hata oluştu!");
            return Result<string>.Unexpected("Dosya yüklenirken bir hata oluştu.");
        }
    }
}
