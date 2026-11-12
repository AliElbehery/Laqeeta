using Laqeeta.Application.Abstractions;
using Laqeeta.Core.Interfaces;
using Laqeeta.Infrastructure.Data;
using Laqeeta.Infrastructure.Repositories;
using Laqeeta.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Laqeeta.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlServer(connectionString)
               .UseSnakeCaseNamingConvention());   // columns => user_id, full_name ... like the ERD

        services.AddScoped<ILostReportRepository, LostReportRepository>();
        services.AddScoped<IFoundItemRepository, FoundItemRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IStationService, StationService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IMatchingService, MatchingService>();
        services.AddScoped<IClaimService, ClaimService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}