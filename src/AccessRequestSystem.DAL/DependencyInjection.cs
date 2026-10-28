using AccessRequestSystem.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccessRequestSystem.DAL;

public static class DependencyInjection
{
    /// <summary>
    /// Регистрирует AppDbContext (PostgreSQL) и репозитории.
    /// Вызывается из Program.cs API-проекта: services.AddDataAccess(configuration).
    /// </summary>
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' не найдена в конфигурации.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IAccessRequestRepository, AccessRequestRepository>();
        services.AddScoped<IAccessLogRepository, AccessLogRepository>();

        return services;
    }
}
