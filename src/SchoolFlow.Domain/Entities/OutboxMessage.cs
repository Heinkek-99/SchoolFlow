using System.Text.Json;

namespace SchoolFlow.Domain.Entities;

/// <summary>
/// Table Outbox — stocke les domain events avant publication.
/// 
/// PROBLÈME SANS OUTBOX :
///   SaveChanges() réussit → Publication event échoue → Event perdu silencieusement
///
/// AVEC OUTBOX :
///   SaveChanges() + OutboxMessage dans la MÊME transaction → 
///   Un worker lit l'Outbox et publie → Garantie at-least-once delivery
///
/// Le Lead Dev a mentionné Pattern Outbox/Inbox — c'est ça.
/// Wolverine intègre ce pattern nativement, mais on peut l'implémenter manuellement.
/// </summary>
public class OutboxMessage : BaseEntity
{
    // public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty; // Nom complet du type
    public string Payload { get; set; } = string.Empty;   // JSON sérialisé
    public DateTime? ProcessedAt { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; } = 0;

    public bool IsProcessed => ProcessedAt.HasValue;
    public bool HasFailed => RetryCount >= 3;

    /// <summary>
    /// Crée un OutboxMessage à partir d'un domain event.
    /// </summary>
    public static OutboxMessage FromDomainEvent(IDomainEvent domainEvent)
    {
        return new OutboxMessage
        {
            EventType = domainEvent.GetType().AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType())
        };
    }

    public void MarquerCommeTraite()
    {
        ProcessedAt = DateTime.UtcNow;
        Error = null;
    }
 
    public void EnregistrerErreur(string message)
    {
        RetryCount++;
        Error = message;
    }
}