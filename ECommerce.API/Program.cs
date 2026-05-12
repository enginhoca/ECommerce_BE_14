using ECommerce.API.Extensions;
using ECommerce.API.Filters;
using ECommerce.API.Middleware;
using ECommerce.Application.DTOs.Common;
using ECommerce.Infrastructure;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/ecommerce-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddDatabase(builder.Configuration);

builder.Services.AddIdentityServices();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddRepositories();

builder.Services.AddApplicationServices();

builder.Services.AddOpenApiDocs();

builder.Services.AddMemoryCache();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCorsPolicy(builder.Configuration);

builder.Services.AddControllers(options => { options.Filters.Add<ApiResponseFilter>(); });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .SelectMany(e => e.Value!.Errors.Select(err =>
                string.IsNullOrEmpty(err.ErrorMessage)
                    ? err.Exception?.Message ?? "Geçersiz değer."
                    : err.ErrorMessage
                ))
            .ToList();
        var response = ApiResponseFactory.Failure(errors, "Validasyon hatası.");
        return new BadRequestObjectResult(response);
    };
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

var exposeOpenApi = app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("OpenApi:EnableInProduction");

if (exposeOpenApi)
{
    app.MapOpenApi(); //  /openapi/v1.json
    app.MapScalarApiReference(options =>
    {
        options.Title = "ECommerce API - Backend 14";
        options.Theme = ScalarTheme.Purple;
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
        options.Authentication = new ScalarAuthenticationOptions
        {
            PreferredSecuritySchemes = ["Bearer"]
        };
    });
    app.UseCors("AllowAll");
}
else
{
    app.UseHsts();
    app.UseCors("AllowSpecific");
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ECommerceDbContext>();
        var userManager = services.GetRequiredService<UserManager<AppIdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await DbInitializer.SeedAsync(context, userManager, roleManager, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Seed işlemi sırasında bir hata oluştu.");
    }
}

app.Run();
