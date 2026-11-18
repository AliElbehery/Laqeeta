using Laqeeta.Application.Abstractions;
using Laqeeta.Core.Interfaces;
using Laqeeta.Infrastructure.Data;
using Laqeeta.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Laqeeta.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlServer(connectionString)
               .UseSnakeCaseNamingConvention());

        services.AddScoped<ILostReportRepository, LostReportRepository>();
        services.AddScoped<IFoundItemRepository, FoundItemRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IMatchingRepository, MatchingRepository>();
        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IStationRepository, StationRepository>();

        return services;
    }
}