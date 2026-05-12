using System;
using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.API.Filters;

public class ApiResponseFilter : IResultFilter
{
    public void OnResultExecuted(ResultExecutedContext context)
    {
        
    }

    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult { Value: Application.DTOs.Common.IApiResponse }) return;

        switch (context.Result)
        {
            case CreatedAtActionResult createdAt:
            {
                // Yönlendirme tabanlı URL üretimi için aktif HTTP isteğinin bağlamı gerekir.
                var httpContext = context.HttpContext;
                // IActionResult üzerinden LinkGenerator yazılmış olabilir; runtime'dan zorunlu olarak alınır.
                var linkGenerator = httpContext.RequestServices.GetRequiredService<LinkGenerator>();

                // Route değerlerine göre tam mutlak URI (scheme + host + path) oluşturulur; Location başlığı buna bağlanır.
                var location = linkGenerator.GetUriByAction(
                    httpContext,
                    createdAt.ActionName,
                    createdAt.ControllerName,
                    createdAt.RouteValues,
                    httpContext.Request.Scheme,
                    httpContext.Request.Host,
                    httpContext.Request.PathBase);

                // Geçerli bir konum URI'si varsa Response.Headers.Location'a yazılır (REST'te oluşturulan kaynağın adresi).
                if (!string.IsNullOrEmpty(location))
                    httpContext.Response.Headers.Location = location;

                // Gövdedeki nesne ya Application Result (IResult) ya da düz dto; uygun ApiResponse üretimine ayrılır.
                object wrapped = createdAt.Value switch
                {
                    // Result pattern: hata/başarı ve mesaj ApiResponseFactory ile tek formata indirgenir.
                    Application.DTOs.Common.IResult ir => ApiResponseFactory.FromResult(ir),
                    // Diğer tüm tipler başarılı sayılır ve Data alanına konur (null dahil).
                    var payload => ApiResponseFactory.Success<object?>(payload)
                };

                // Orijinal CreatedAtActionResult yerine aynı status ile sarılmış ObjectResult konur.
                context.Result = new ObjectResult(wrapped)
                {
                    // StatusCode yoksa REST varsayılanı 201 Created kullanılır.
                    StatusCode = createdAt.StatusCode ?? StatusCodes.Status201Created
                };
                break;
            }
            case ObjectResult { StatusCode: >= 200 and <300 } successResult:
                {
                    var wrapped = new ApiResponse<object>
                    {
                        IsSuccess = true,
                        Data = successResult.Value,
                        Message = null
                    };
                    context.Result = new ObjectResult(wrapped)
                    {
                        StatusCode = successResult.StatusCode
                    };
                }
                break;
            case ObjectResult errorResult when errorResult.StatusCode >= 400:
                {
                    var message = errorResult.Value?.ToString() ?? "Bir hata oluştu.";
                    var wrapped = ApiResponseFactory.Failure(message);
                    context.Result = new ObjectResult(wrapped)
                    {
                        StatusCode = errorResult.StatusCode
                    };
                }
                break;
            case StatusCodeResult { StatusCode: 204 }:
                context.Result = new ObjectResult(ApiResponseFactory.Success("İşlem başarılı"))
                {
                    StatusCode = 200
                };
                break;
            case StatusCodeResult statusCodeResult when statusCodeResult.StatusCode>=400:
                {
                    var message = statusCodeResult.StatusCode switch
                    {
                        401     => "Kimlik doğrulama gereklidir!",
                        403     => "Bu işlem için yetkiniz yok!",
                        404     => "Kaynak bulunamadı!",
                        _ => "Bir hata oluştu."
                    };
                    context.Result = new ObjectResult(ApiResponseFactory.Failure(message))
                    {
                        StatusCode= statusCodeResult.StatusCode
                    };
                }
                break;
            default:
                {

                }
                break;
        }
    }

    private static int MapErrorTypeToStatusCode(ResultErrorType errorType) => 
        errorType switch
        {
            ResultErrorType.NotFound        => 404,
            ResultErrorType.Validation      => 400,
            ResultErrorType.Conflict        => 409,
            ResultErrorType.Unauthorized    => 401,
            ResultErrorType.Forbidden       => 403,
            ResultErrorType.Unexpected      => 500,
            _ => 200  
        };
}
