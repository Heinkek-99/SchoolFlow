using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Domain.Entities;
using System.Text.Json;

namespace SchoolFlow.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

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

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Corriger tous les DateTime Unspecified avant sauvegarde
        foreach (var entry in ChangeTracker.Entries())
        {
            foreach (var property in entry.Properties)
            {
                if (property.CurrentValue is DateTime dt && 
                    dt.Kind == DateTimeKind.Unspecified)
                {
                    property.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                }
            }
        }
        
        return await base.SaveChangesAsync(ct);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);       

        // Configuration Utilisateur
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
            
            entity.HasQueryFilter(u => !u.IsArchived); // Global filter
        });

        // Configuration Famille
        modelBuilder.Entity<Famille>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NomPere).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PrenomPere).HasMaxLength(100);
            entity.Property(e => e.TelephonePere).HasMaxLength(20);
            entity.Property(e => e.EmailPere).HasMaxLength(200);
            entity.Property(e => e.NomMere).HasMaxLength(100);
            entity.Property(e => e.PrenomMere).HasMaxLength(100);
            entity.Property(e => e.Adresse).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Ville).HasMaxLength(100).IsRequired();
            entity.Property(e => e.TelephonePrincipal).HasMaxLength(20).IsRequired();
            
            entity.HasMany(f => f.Eleves)
                .WithOne(e => e.Famille)
                .HasForeignKey(e => e.FamilleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(f => f.Paiements)
                .WithOne(p => p.Famille)
                .HasForeignKey(p => p.FamilleId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.Ignore(f => f.NomFamille);
            entity.Ignore(f => f.TotalDu);
            entity.Ignore(f => f.TotalPaye);
            entity.Ignore(f => f.SoldeGlobal);
            entity.Ignore(f => f.TauxRecouvrement);
            entity.Ignore(f => f.JoursDepuisDernierPaiement);
            entity.Ignore(f => f.DateDernierPaiement);
            entity.Ignore(f => f.NombreEnfantsActifs);
            entity.Ignore(f => f.StatutPaiement);
            
            entity.HasQueryFilter(f => !f.IsArchived);
        });

        // Configuration Eleve
        modelBuilder.Entity<Eleve>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Matricule).IsUnique();
            entity.Property(e => e.Matricule).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Nom).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Prenom).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LieuNaissance).HasMaxLength(200).IsRequired();
            entity.Property(e => e.PhotoPath).HasMaxLength(500);
            entity.Property(e => e.Sexe).HasConversion<string>();
            entity.Property(e => e.Statut).HasConversion<string>();
            
            entity.HasOne(e => e.Famille)
                .WithMany(f => f.Eleves)
                .HasForeignKey(e => e.FamilleId)
                .OnDelete(DeleteBehavior.Restrict); 

            entity.HasOne(e => e.Classe)
                .WithMany(c => c.Eleves)
                .HasForeignKey(e => e.ClasseId)
                .OnDelete(DeleteBehavior.Restrict); 

            entity.HasOne(e => e.AnneeScolaire)
                .WithMany(a => a.Eleves)
                .HasForeignKey(e => e.AnneeScolaireId)
                .OnDelete(DeleteBehavior.Restrict); 

            entity.HasMany(e => e.Frais)
                .WithOne(f => f.Eleve)
                .HasForeignKey(f => f.EleveId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Ignore(e => e.NomComplet);
            entity.Ignore(e => e.Age);
            entity.Ignore(e => e.TotalDu);
            entity.Ignore(e => e.TotalPaye);
            entity.Ignore(e => e.Solde);
            entity.Ignore(e => e.EstAJour);
            entity.Ignore(e => e.PourcentagePaye);
            entity.Ignore(e => e.NombreFraisImpayes);
            entity.Ignore(e => e.FraisPlusAncienImpaye);
            // entity.Ignore(e => e.MoyenneGenerale);
            
            entity.HasQueryFilter(e => !e.IsArchived);
        });

        // Configuration Classe
        modelBuilder.Entity<Classe>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Nom).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Niveau).HasConversion<string>();
            entity.Property(e => e.Section).HasMaxLength(20);
            
            entity.HasOne(c => c.AnneeScolaire)
                .WithMany(a => a.Classes)
                .HasForeignKey(c => c.AnneeScolaireId)
                .OnDelete(DeleteBehavior.Restrict); 
            
            entity.Ignore(c => c.EffectifActuel);
            
            entity.HasQueryFilter(c => !c.IsArchived);
        });

        // Configuration TypeFrais
        modelBuilder.Entity<TypeFrais>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .IsRequired();
            entity.Property(e => e.Libelle)
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(e => e.Categorie)
                .HasConversion<string>();
            
            // Sérialisation JSON pour Dictionary
            entity.Property(e => e.MontantsParNiveau)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<Niveau, decimal>>(v, (JsonSerializerOptions?)null) 
                         ?? new Dictionary<Niveau, decimal>()
                )
                .Metadata.SetValueComparer(
                    new ValueComparer<Dictionary<Niveau, decimal>>(
                        (c1, c2) => c1!.SequenceEqual(c2!),  // Comparaison
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),  // Hash
                        c => c.ToDictionary(e => e.Key, e => e.Value)  // Snapshot
            )
        );
            
            entity.Property(e => e.MontantsParNiveau)
                .HasColumnType("jsonb");

            entity.HasQueryFilter(t => !t.IsArchived);
        });

        // Configuration Frais
        modelBuilder.Entity<Frais>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Montant).HasPrecision(18, 2);
            entity.Property(e => e.MontantPaye).HasPrecision(18, 2);
            
            entity.HasOne(f => f.Eleve)
                .WithMany(e => e.Frais)
                .HasForeignKey(f => f.EleveId)
                .OnDelete(DeleteBehavior.Restrict); 

            entity.HasOne(f => f.TypeFrais)
                .WithMany(t => t.Frais)
                .HasForeignKey(f => f.TypeFraisId)
                .OnDelete(DeleteBehavior.Restrict); 

            entity.HasOne(f => f.Periode)
                .WithMany()
                .HasForeignKey(f => f.PeriodeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(f => f.Ventilations)
                .WithOne(v => v.Frais)
                .HasForeignKey(v => v.FraisId)
                .OnDelete(DeleteBehavior.Restrict); 
            
            entity.Ignore(f => f.Solde);
            entity.Ignore(f => f.EstSolde);
            entity.Ignore(f => f.IsEchu);
            entity.Ignore(f => f.JoursRetard);
            entity.Ignore(f => f.PourcentagePaye);
            entity.Ignore(f => f.StatutPaiement);
            
            entity.HasQueryFilter(f => !f.IsArchived);
        });

        // Configuration Paiement
        modelBuilder.Entity<Paiement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.NumeroPaiement).IsUnique();
            entity.Property(e => e.NumeroPaiement).HasMaxLength(30).IsRequired();
            entity.Property(e => e.MontantTotal).HasPrecision(18, 2);
            entity.Property(e => e.ModePaiement).HasConversion<string>();
            entity.Property(e => e.Reference).HasMaxLength(100);
            
            entity.HasOne(p => p.Famille)
                .WithMany(f => f.Paiements)
                .HasForeignKey(p => p.FamilleId)
                .OnDelete(DeleteBehavior.Restrict); 

            entity.HasOne(p => p.EnregistreParUtilisateur)
                .WithMany()
                .HasForeignKey(p => p.EnregistrePar)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(p => p.Ventilations)
                .WithOne(v => v.Paiement)
                .HasForeignKey(v => v.PaiementId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Ignore(p => p.IsVentilationComplete);
            entity.Ignore(p => p.NombreElevesBeneficiaires);
            
            entity.HasQueryFilter(p => !p.IsArchived);
        });

        // Configuration VentilationPaiement
        modelBuilder.Entity<VentilationPaiement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MontantVentile)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.Remarque)
                .HasMaxLength(500);
            
            entity.HasOne(v => v.Paiement)
                .WithMany(p => p.Ventilations)
                .HasForeignKey(v => v.PaiementId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.Eleve)
                .WithMany()
                .HasForeignKey(v => v.EleveId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.Frais)
                .WithMany(f => f.Ventilations)
                .HasForeignKey(v => v.FraisId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Index pour performance
            entity.HasIndex(v => v.PaiementId);
            entity.HasIndex(v => v.EleveId);
            entity.HasIndex(v => v.FraisId);

            entity.HasQueryFilter(v => !v.IsArchived);
        });

        // Configuration Note
        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Valeur).HasPrecision(5, 2);
            entity.Property(e => e.NoteSur).HasPrecision(5, 2);
            entity.Property(e => e.Type).HasConversion<string>();
            
            entity.HasOne(n => n.Eleve)
                .WithMany(e => e.Notes)
                .HasForeignKey(n => n.EleveId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


            entity.HasOne(n => n.Matiere)
                .WithMany(m => m.Notes)
                .HasForeignKey(n => n.MatiereId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(n => n.Periode)
                .WithMany()
                .HasForeignKey(n => n.PeriodeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuration AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action)
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(e => e.EntityType)
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(e => e.IpAddress)
                .HasMaxLength(50);
            entity.Property(e => e.OldValues)
                .HasColumnType("text");
            entity.Property(e => e.NewValues)
                .HasColumnType("text");
            
            entity.HasOne(a => a.Utilisateur)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UtilisateurId)
                .IsRequired(false) 
                .OnDelete(DeleteBehavior.SetNull);

             // Index pour performance
            entity.HasIndex(a => a.UtilisateurId);
            entity.HasIndex(a => a.EntityType);
            entity.HasIndex(a => a.CreatedAt);
        });

        ConfigureOtherEntities(modelBuilder);
        
        // Seed Data
        SeedData(modelBuilder);
    }

    private void ConfigureOtherEntities(ModelBuilder modelBuilder)
    {
        // Additional entity configurations can be added here
        // Configuration AnneeScolaire
        modelBuilder.Entity<AnneeScolaire>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Libelle).HasMaxLength(20).IsRequired();
            
            entity.HasMany(a => a.Periodes)
                .WithOne(p => p.AnneeScolaire)
                .HasForeignKey(p => p.AnneeScolaireId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuration Periode
        modelBuilder.Entity<Periode>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Libelle).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Type).HasConversion<string>();
        });

        // Configuration Matiere
        modelBuilder.Entity<Matiere>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Libelle).HasMaxLength(100).IsRequired();
        });

    }
       private void SeedData(ModelBuilder modelBuilder)
    {
        // Année scolaire 2024-2025
        var anneeScolaireId = Guid.NewGuid();
        modelBuilder.Entity<AnneeScolaire>().HasData(new AnneeScolaire
        {
            Id = anneeScolaireId,
            Libelle = "2024-2025",
            DateDebut = new DateTime(2024, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            DateFin = new DateTime(2025, 6, 30, 23, 59, 59, DateTimeKind.Utc),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Périodes (Trimestres)
        var trimestre1Id = Guid.NewGuid();
        var trimestre2Id = Guid.NewGuid();
        var trimestre3Id = Guid.NewGuid();

        modelBuilder.Entity<Periode>().HasData(
            new Periode
            {
                Id = trimestre1Id,
                Libelle = "Trimestre 1",
                Type = TypePeriode.Trimestre,
                Numero = 1,
                DateDebut = new DateTime(2024, 9, 1,  0, 0, 0, DateTimeKind.Utc),
                DateFin = new DateTime(2024, 12, 15, 23, 59, 59, DateTimeKind.Utc),
                AnneeScolaireId = anneeScolaireId,
                CreatedAt = DateTime.UtcNow
            },
            new Periode
            {
                Id = trimestre2Id,
                Libelle = "Trimestre 2",
                Type = TypePeriode.Trimestre,
                Numero = 2,
                DateDebut = new DateTime(2025, 1, 7, 0, 0, 0, DateTimeKind.Utc),
                DateFin = new DateTime(2025, 3, 31, 23, 59, 59, DateTimeKind.Utc),
                AnneeScolaireId = anneeScolaireId,
                CreatedAt = DateTime.UtcNow
            },
            new Periode
            {
                Id = trimestre3Id,
                Libelle = "Trimestre 3",
                Type = TypePeriode.Trimestre,
                Numero = 3,
                DateDebut = new DateTime(2025, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                DateFin = new DateTime(2025, 6, 30, 23, 59, 59, DateTimeKind.Utc),
                AnneeScolaireId = anneeScolaireId,
                CreatedAt = DateTime.UtcNow
            }
        );

        // Utilisateur Admin par défaut
        var adminId = Guid.NewGuid();
        var directorId = Guid.NewGuid();
        var accountantId = Guid.NewGuid();
        var secretaryId = Guid.NewGuid();

        modelBuilder.Entity<Utilisateur>().HasData(
            new Utilisateur
            {
                Id = adminId,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@2025"),
                Nom = "Administrateur",
                Prenom = "Système",
                Email = "admin@schoolflow.com",
                Role = Role.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Utilisateur
            {
                Id = directorId,
                Username = "directeur",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Dir@2025"),
                Nom = "Durand",
                Prenom = "Marie",
                Email = "directeur@schoolflow.com",
                Role = Role.Directeur,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Utilisateur
            {
                Id = accountantId,
                Username = "comptable",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Compta@2025"),
                Nom = "Martin",
                Prenom = "Sophie",
                Email = "comptable@schoolflow.com",
                Role = Role.Comptable,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Utilisateur
            {
                Id = secretaryId,
                Username = "secretaire",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Sec@2025"),
                Nom = "Leroy",
                Prenom = "Paul",
                Email = "secretaire@schoolflow.com",
                Role = Role.Secretaire,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );

        // Classes standards
        var classes = new List<Classe>();
        var niveaux = Enum.GetValues<Niveau>();
        
        foreach (var niveau in niveaux)
        {
            classes.Add(new Classe
            {
                Id = Guid.NewGuid(),
                Code = niveau.ToString().ToUpper(),
                Nom = GetNiveauLibelle(niveau),
                Niveau = niveau,
                CapaciteMax = 60,
                AnneeScolaireId = anneeScolaireId,
                CreatedAt = DateTime.UtcNow
            });
        }
        modelBuilder.Entity<Classe>().HasData(classes);

        // Types de frais standards
        var typeInscription = Guid.NewGuid();
        var typeScolarite = Guid.NewGuid();
        var typeCantine = Guid.NewGuid();

        var montantsInscription = niveaux.ToDictionary(n => n, n => 5000m);
        var montantsScolarite = niveaux.ToDictionary(n => n, n => GetMontantScolarite(n));
        var montantsCantine = niveaux.ToDictionary(n => n, n => 15000m);

        modelBuilder.Entity<TypeFrais>().HasData(
            new TypeFrais
            {
                Id = typeInscription,
                Code = "INSC",
                Libelle = "Frais d'inscription",
                Categorie = CategorieFrais.Inscription,
                IsRecurrent = false,
                IsObligatoire = true,
                GenerationAutomatique = true,
                MontantsParNiveau = montantsInscription,
                CreatedAt = DateTime.UtcNow
            },
            new TypeFrais
            {
                Id = typeScolarite,
                Code = "SCOL",
                Libelle = "Scolarité",
                Description = "Frais de scolarité trimestriel",
                Categorie = CategorieFrais.Scolarite,
                IsRecurrent = true,
                IsObligatoire = true,
                GenerationAutomatique = true,
                MontantsParNiveau = montantsScolarite,
                CreatedAt = DateTime.UtcNow
            },
            new TypeFrais
            {
                Id = typeCantine,
                Code = "CANT",
                Libelle = "Cantine",
                Description = "Frais de cantine mensuel",
                Categorie = CategorieFrais.Cantine,
                IsRecurrent = true,
                IsObligatoire = false,
                GenerationAutomatique = false,
                MontantsParNiveau = montantsCantine,
                CreatedAt = DateTime.UtcNow
            }
        );
    }

    private string GetNiveauLibelle(Niveau niveau) => niveau switch
    {
        Niveau.CP => "CP",
        Niveau.CE1 => "CE1",
        Niveau.CE2 => "CE2",
        Niveau.CM1 => "CM1",
        Niveau.CM2 => "CM2",
        Niveau.Sixieme => "6ème",
        Niveau.Cinquieme => "5ème",
        Niveau.Quatrieme => "4ème",
        Niveau.Troisieme => "3ème",
        Niveau.Seconde => "2nde",
        Niveau.Premiere => "1ère",
        Niveau.Terminale => "Tle",
        _ => niveau.ToString()
    };

    private decimal GetMontantScolarite(Niveau niveau) => niveau switch
    {
        Niveau.CP or Niveau.CE1 or Niveau.CE2 => 60000,
        Niveau.CM1 or Niveau.CM2 => 70000,
        Niveau.Sixieme or Niveau.Cinquieme => 80000,
        Niveau.Quatrieme or Niveau.Troisieme => 85000,
        Niveau.Seconde or Niveau.Premiere or Niveau.Terminale => 90000,
        _ => 75000
    };
}