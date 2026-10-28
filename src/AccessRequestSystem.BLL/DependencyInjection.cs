using AccessRequestSystem.BLL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AccessRequestSystem.BLL;

public static class DependencyInjection
{
    /// <summary>Вызывается из Program.cs API-проекта: services.AddBusinessLogic();</summary>
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
        services.AddScoped<IAccessRequestService, AccessRequestService>();
        services.AddScoped<IProvisioningService, AccessProvisioningService>();
        services.AddScoped<IAccessLogService, AccessLogService>();

        return services;
    }
}
