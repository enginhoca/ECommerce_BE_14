using System;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ECommerceDbContext>
{
    public ECommerceDbContext CreateDbContext(string[] args)
    {
        // 1.Adım: Konfigürasyon Yapılandırması
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../ECommerce.API"))
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // 2.Adım: ConnectionString'ş okumak
        var connectionString = configuration.GetConnectionString("PostgresConnection");

        // 3.Adım: DbContext Seçeneklerini Yapılandırmak
        var optionsBuilder = new DbContextOptionsBuilder<ECommerceDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ECommerceDbContext(optionsBuilder.Options);
    }
}
