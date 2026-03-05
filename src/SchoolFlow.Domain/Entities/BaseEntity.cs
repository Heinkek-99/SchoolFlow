using System.Runtime.CompilerServices;

namespace SchoolFlow.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    private DateTime _createdAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt 
    { 
        get => _createdAt;
        set => _createdAt = value.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc) 
            : value.ToUniversalTime();
    }    
    private DateTime? _updatedAt { get; set; }
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
}