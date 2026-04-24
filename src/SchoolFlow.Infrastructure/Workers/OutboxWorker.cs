// ============================================================
// FICHIER : src/SchoolFlow.Infrastructure/Workers/OutboxWorker.cs
// ACTION  : Fichier à CRÉER (nouveau)
// RÔLE    : Lit les OutboxMessages non traités et les republique.
//           Garantit qu'aucun event n'est perdu même si le process crash.
// ============================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Domain.Entities;
using SchoolFlow.Infrastructure.Data;
using System.Text.Json;

namespace SchoolFlow.Infrastructure.Workers;

/// <summary>
/// Worker de fond — vérifie toutes les 30 secondes si des OutboxMessages
/// n'ont pas été traités (ProcessedAt IS NULL) et les publie.
///
/// Ce scénario arrive quand :
///   1. SaveChanges réussit (données + OutboxMessage sauvegardés)
///   2. Publication MediatR échoue (exception, timeout réseau, etc.)
///   3. Le worker reprend et retry la publication
/// </summary>
public class OutboxWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

    public OutboxWorker(IServiceProvider serviceProvider, ILogger<OutboxWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🔄 OutboxWorker démarré.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur dans le OutboxWorker.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

        // Récupérer les messages non traités (max 20 par cycle, retry < 3)
        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.RetryCount < 3)
            .OrderBy(m => m.CreatedAt)
            .Take(20)
            .ToListAsync(ct);

        if (!messages.Any()) return;

        _logger.LogInformation("📬 {Count} OutboxMessage(s) à traiter.", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.EventType);
                if (eventType is null)
                {
                    _logger.LogWarning("Type inconnu: {EventType}", message.EventType);
                    message.EnregistrerErreur($"Type inconnu: {message.EventType}");
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType) as IDomainEvent;
                if (domainEvent is null)
                {
                    message.EnregistrerErreur("Désérialisation échouée.");
                    continue;
                }

                await dispatcher.DispatchAsync(new[] { domainEvent }, ct);
                message.MarquerCommeTraite();

                _logger.LogInformation("✅ Event publié: {EventType}", eventType.Name);
            }
            catch (Exception ex)
            {
                message.EnregistrerErreur(ex.Message);
                _logger.LogWarning("⚠️ Echec publication {EventType}: {Error}",
                    message.EventType, ex.Message);
            }
        }

        await dbContext.SaveChangesAsync(ct);
    }
}