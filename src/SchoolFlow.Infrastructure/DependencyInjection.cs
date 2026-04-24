// ============================================================
// FICHIER : src/SchoolFlow.Infrastructure/DependencyInjection.cs
// FIX     : Enregistrement de IDomainEventDispatcher
// ============================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Services;
using SchoolFlow.Application.Services;
using SchoolFlow.Infrastructure.Data;
using SchoolFlow.Infrastructure.Events;
using SchoolFlow.Infrastructure.Services;
using SchoolFlow.Infrastructure.Workers;

namespace SchoolFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // ── DbContext ─────────────────────────────────────────────────────────
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString,
                b =>
                {
                    b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    b.EnableRetryOnFailure(3);
                    b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                })
        );

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        // ── Domain Event Dispatcher ────────────────────────────────────────────
        // C'est lui qui wraps IDomainEvent → DomainEventNotification<T> → MediatR
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // ── Dapper Read Service ───────────────────────────────────────────────
        services.AddScoped<ISchoolFlowReadService>(_ =>
            new Data.Dapper.SchoolFlowReadService(connectionString!));

        // ── Services ─────────────────────────────────────────────────────────
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // ── Seeder (DEV uniquement) ───────────────────────────────────────────
        services.AddScoped<DatabaseSeeder>();

        // ── Outbox Worker ─────────────────────────────────────────────────────
        services.AddHostedService<OutboxWorker>();

        return services;
    }
}