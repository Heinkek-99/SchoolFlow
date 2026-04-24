using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Domain.Entities;
 
namespace SchoolFlow.Infrastructure.Data;
 
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly IDomainEventDispatcher _dispatcher;
 
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDomainEventDispatcher dispatcher)
        : base(options)
    {
        _dispatcher = dispatcher;
    }
 
    // ── DbSets ───────────────────────────────────────────────────────────────
    public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();
    public DbSet<Famille> Familles => Set<Famille>();
    public DbSet<Eleve> Eleves => Set<Eleve>();
    public DbSet<Classe> Classes => Set<Classe>();
    public DbSet<AnneeScolaire> AnneeScolaires => Set<AnneeScolaire>();
    public DbSet<Periode> Periodes => Set<Periode>();
    public DbSet<TypeFrais> TypeFrais => Set<TypeFrais>();
    public DbSet<Frais> Frais => Set<Frais>();
    public DbSet<Paiement> Paiements => Set<Paiement>();
    public DbSet<VentilationPaiement> VentilationsPaiement => Set<VentilationPaiement>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<Matiere> Matieres => Set<Matiere>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Ecole> Ecoles => Set<Ecole>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
 
    // ── SaveChangesAsync ─────────────────────────────────────────────────────
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        NormaliserDateTimeUtc();
 
        // Collecter tous les events AVANT le save
        var entitesAvecEvents = ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();
 
        var tousLesEvents = entitesAvecEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();
 
        // Persister les OutboxMessages dans la même transaction
        foreach (var evt in tousLesEvents)
            OutboxMessages.Add(OutboxMessage.FromDomainEvent(evt));
 
        var result = await base.SaveChangesAsync(ct);
 
        // Dispatcher après le commit
        if (tousLesEvents.Any())
        {
            await _dispatcher.DispatchAsync(tousLesEvents, ct);
 
            foreach (var entity in entitesAvecEvents)
                entity.ClearDomainEvents();
        }
 
        return result;
    }
 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
 
        // ── OUTBOX ────────────────────────────────────────────────────────────
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasMaxLength(500).IsRequired();
            entity.HasIndex(e => e.ProcessedAt);
        });
 
        // ── ECOLE ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Ecole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nom).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CodeEcole).HasMaxLength(30).IsRequired();
            entity.HasIndex(e => e.CodeEcole).IsUnique();
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.TypeSecteur).HasConversion<string>();
            entity.Property(e => e.SousSysteme).HasConversion<string>();
            entity.Property(e => e.Statut).HasConversion<string>();
            entity.Property(e => e.Slug).HasMaxLength(100);
            entity.Property(e => e.CouleurPrimaire).HasMaxLength(10);
            entity.Property(e => e.CouleurSecondaire).HasMaxLength(10);
            entity.Property(e => e.TelephonePrincipal).HasMaxLength(20).IsRequired();
            entity.Property(e => e.NomDirecteur).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PrenomDirecteur).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Adresse).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Ville).HasMaxLength(100).IsRequired();
        });
 
        // ── UTILISATEUR ───────────────────────────────────────────────────────
        modelBuilder.Entity<Utilisateur>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Nom).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Prenom).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Role).HasConversion<string>();
            entity.HasOne(u => u.Ecole)
                .WithMany(e => e.Utilisateurs)
                .HasForeignKey(u => u.EcoleId)
                .IsRequired(false)  // null pour SuperAdmin
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(u => !u.IsArchived);
        });
 
        // ── FAMILLE ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Famille>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NomPere).HasMaxLength(100).IsRequired();
            entity.Property(e => e.TelephonePrincipal).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Adresse).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Ville).HasMaxLength(100).IsRequired();
            entity.HasMany(f => f.Eleves).WithOne(e => e.Famille)
                .HasForeignKey(e => e.FamilleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(f => f.Paiements).WithOne(p => p.Famille)
                .HasForeignKey(p => p.FamilleId).OnDelete(DeleteBehavior.Restrict);
            entity.Ignore(f => f.NomFamille);
            entity.Ignore(f => f.TotalDu);
            entity.Ignore(f => f.TotalPaye);
            entity.Ignore(f => f.SoldeGlobal);
            entity.Ignore(f => f.TauxRecouvrement);
            entity.Ignore(f => f.StatutPaiement);
            entity.Ignore(f => f.NombreEnfantsActifs);
            entity.Ignore(f => f.DateDernierPaiement);
            entity.HasQueryFilter(f => !f.IsArchived);
        });
 
        // ── ÉLÈVE ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Eleve>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Matricule).IsUnique();
            entity.Property(e => e.Matricule).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Nom).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Prenom).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Sexe).HasConversion<string>();
            entity.Property(e => e.Statut).HasConversion<string>();
            entity.Ignore(e => e.NomComplet);
            entity.Ignore(e => e.Age);
            entity.Ignore(e => e.TotalDu);
            entity.Ignore(e => e.TotalPaye);
            entity.Ignore(e => e.Solde);
            entity.Ignore(e => e.EstAJour);
            entity.Ignore(e => e.PourcentagePaye);
            entity.Ignore(e => e.NombreFraisImpayes);
            entity.Ignore(e => e.FraisPlusAncienImpaye);
            entity.HasQueryFilter(e => !e.IsArchived);
        });
 
        // ── CLASSE ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Classe>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Nom).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Section).HasMaxLength(20);
            entity.Property(e => e.Niveau).HasConversion<string>();
            entity.Property(e => e.SousSysteme).HasConversion<string>();
            entity.Property(e => e.Statut).HasConversion<string>();
            entity.Property(e => e.FraisScolarite).HasColumnType("decimal(18,2)");
            entity.Ignore(e => e.EffectifActuel);
            entity.Ignore(e => e.EstPleine);
            entity.Ignore(e => e.PlacesDisponibles);
            entity.Ignore(e => e.NomComplet);
        });
 
        // ── FRAIS ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Frais>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Montant).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MontantPaye).HasColumnType("decimal(18,2)");
            entity.Ignore(e => e.Solde);
            entity.Ignore(e => e.EstSolde);
            entity.Ignore(e => e.IsEchu);
            entity.Ignore(e => e.JoursRetard);
            entity.Ignore(e => e.PourcentagePaye);
            entity.Ignore(e => e.StatutPaiement);
            // PeriodeId nullable — les frais d'inscription n'ont pas de période
            entity.HasOne(f => f.Periode)
                .WithMany()
                .HasForeignKey(f => f.PeriodeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(f => !f.IsArchived);
        });
 
        // ── TYPE FRAIS ────────────────────────────────────────────────────────
        modelBuilder.Entity<TypeFrais>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Libelle).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Categorie).HasConversion<string>();
            entity.Property(e => e.MontantsParNiveau)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v,
                        (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<Niveau, decimal>>(v,
                        (System.Text.Json.JsonSerializerOptions?)null) ?? new(),
                    new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<Dictionary<Niveau, decimal>>(
                        (c1, c2) => System.Text.Json.JsonSerializer.Serialize(c1, (System.Text.Json.JsonSerializerOptions?)null) ==
                                    System.Text.Json.JsonSerializer.Serialize(c2, (System.Text.Json.JsonSerializerOptions?)null),
                        c => System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null).GetHashCode(),
                        c => System.Text.Json.JsonSerializer.Deserialize<Dictionary<Niveau, decimal>>(
                            System.Text.Json.JsonSerializer.Serialize(c, (System.Text.Json.JsonSerializerOptions?)null),
                            (System.Text.Json.JsonSerializerOptions?)null) ?? new()));
            entity.HasQueryFilter(tf => !tf.IsArchived);
        });
 
        // ── PAIEMENT ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Paiement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.NumeroPaiement).IsUnique();
            entity.Property(e => e.NumeroPaiement).HasMaxLength(30).IsRequired();
            entity.Property(e => e.MontantTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ModePaiement).HasConversion<string>();
            entity.Ignore(e => e.IsVentilationComplete);
            entity.Ignore(e => e.NombreElevesBeneficiaires);
            entity.HasMany(p => p.Ventilations).WithOne(v => v.Paiement)
                .HasForeignKey(v => v.PaiementId).OnDelete(DeleteBehavior.Cascade);
            // IsRequired(false) évite le warning EF "query filter on required end"
            entity.HasOne(p => p.EnregistreParUtilisateur)
                .WithMany()
                .HasForeignKey(p => p.EnregistrePar)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });
 
        // ── VENTILATION ───────────────────────────────────────────────────────
        modelBuilder.Entity<VentilationPaiement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MontantVentile).HasColumnType("decimal(18,2)");
            // IsRequired(false) évite le warning EF "query filter on required end"
            entity.HasOne(v => v.Eleve)
                .WithMany()
                .HasForeignKey(v => v.EleveId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });
 
        // ── ANNÉE SCOLAIRE ────────────────────────────────────────────────────
        modelBuilder.Entity<AnneeScolaire>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(a => a.Periodes).WithOne(p => p.AnneeScolaire)
                .HasForeignKey(p => p.AnneeScolaireId).OnDelete(DeleteBehavior.Cascade);
        });
 
        // ── AUDIT LOG ────────────────────────────────────────────────────────
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // ── NOTE ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Valeur).HasColumnType("decimal(5,2)");
            entity.Property(e => e.NoteSur).HasColumnType("decimal(5,2)");
        });

        // ── MATIERE ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Matiere>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Libelle).HasMaxLength(100).IsRequired();
        });

        // ── PERIODE ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Periode>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Libelle).HasMaxLength(100).IsRequired();
        });
    }
 
    private void NormaliserDateTimeUtc()
    {
        foreach (var entry in ChangeTracker.Entries())
        foreach (var property in entry.Properties)
            if (property.CurrentValue is DateTime dt && dt.Kind == DateTimeKind.Unspecified)
                property.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    }
}