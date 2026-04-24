namespace SchoolFlow.Domain.Entities;

// ─── INTERFACE DOMAIN EVENT ─────────────────────────────────────────────────

/// <summary>
/// Marker interface — tous les domain events implémentent ceci.
/// Compatible INotification pour publication via MediatR.
/// </summary>
public interface IDomainEvent 
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}

/// <summary>
/// Classe de base record pour les domain events.
/// Utilise record pour l'égalité structurelle (pratique pour les tests).
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

// ─── BASE ENTITY ─────────────────────────────────────────────────────────────

/// <summary>
/// Entité de base avec support DomainEvents intégré + audit complet.
///
/// CHANGEMENTS vs l'ancienne version :
/// - Ajout _domainEvents (liste privée) 
/// - Méthodes RaiseDomainEvent / ClearDomainEvents
/// - Setters UTC conservés (compatibles avec ta correction Npgsql existante)
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    private DateTime _createdAt = DateTime.UtcNow;
    public DateTime CreatedAt
    {
        get => _createdAt;
        set => _createdAt = value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();
    }

    private DateTime? _updatedAt;
    public DateTime? UpdatedAt
    {
        get => _updatedAt;
        set => _updatedAt = value.HasValue
            ? (value.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                : value.Value.ToUniversalTime())
            : null;
    }

    public bool IsArchived { get; set; } = false;
    public DateTime? ArchivedAt { get; set; }
    public Guid? ArchivedBy { get; set; }
    public string? ArchiveReason { get; set; }

    // ── DOMAIN EVENTS ────────────────────────────────────────────────────────
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>Liste en lecture seule des events en attente de publication.</summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Lève un domain event — appelé depuis les méthodes métier de l'entité.</summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    /// <summary>Vide les events après publication — appelé par le DbContext.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}

// ─── TENANT ENTITY ────────────────────────────────────────────────────────────

/// <summary>
/// Entité avec isolation par établissement (multi-tenant).
/// TOUTES les entités métier doivent hériter de TenantEntity (pas BaseEntity).
/// BaseEntity reste pour les entités système (OutboxMessage, AuditLog, Ecole).
/// </summary>
public abstract class TenantEntity : BaseEntity
{
    /// <summary>
    /// Identifiant de l'école propriétaire de cette donnée.
    /// Filtre appliqué automatiquement par le DbContext via ICurrentUserService.
    /// </summary>
    public Guid EcoleId { get; set; }
}