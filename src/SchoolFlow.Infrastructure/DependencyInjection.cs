using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Services;
using SchoolFlow.Infrastructure.Data;
using SchoolFlow.Infrastructure.Services;

namespace SchoolFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            )
        );

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // JWT Service
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // File Storage Service — upload photos élèves
        services.AddHttpContextAccessor();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}