using Laqeeta.Application.Services;
using Laqeeta.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Laqeeta.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ILostReportService, LostReportService>();
        services.AddScoped<IFoundItemService, FoundItemService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAdminService, AdminService>();
        return services;
    }
}
