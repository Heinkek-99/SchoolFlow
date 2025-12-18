using Microsoft.EntityFrameworkCore;
using SchoolFlow.Domain.Entities;
using SchoolFlow.Infrastructure.Data;

namespace Application.Tests.Common;

/// <summary>
/// Factory pour créer un DbContext In-Memory pour les tests
/// </summary>
public static class ApplicationDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Base unique par test
            .Options;

        var context = new ApplicationDbContext(options);

        // Seed data de base
        SeedTestData(context);

        return context;
    }

    private static void SeedTestData(ApplicationDbContext context)
    {
        // Année scolaire
        var anneeScolaire = new AnneeScolaire
        {
            Id = Guid.NewGuid(),
            Libelle = "2024-2025",
            DateDebut = new DateTime(2024, 9, 1),
            DateFin = new DateTime(2025, 6, 30),
            EstActive = true
        };
        context.AnneesScolaires.Add(anneeScolaire);

        // Classe
        var classe = new Classe
        {
            Id = Guid.NewGuid(),
            Nom = "6ème A",
            Niveau = NiveauScolaire.College,
            Capacite = 35,
            AnneeScolaireId = anneeScolaire.Id
        };
        context.Classes.Add(classe);

        // Famille de test
        var famille = new Famille
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            NomPere = "TRAORE",
            PrenomPere = "Amadou",
            TelephonePere = "+225 07 12 34 56 78",
            NomMere = "KONE",
            PrenomMere = "Fatou",
            TelephoneMere = "+225 05 98 76 54 32",
            Adresse = "Cocody Angré",
            Ville = "Abidjan"
        };
        context.Familles.Add(famille);

        // Élève de test
        var eleve = new Eleve
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Matricule = "EL202400001",
            Nom = "TRAORE",
            Prenom = "Mohamed",
            DateNaissance = new DateTime(2012, 5, 15),
            Sexe = Sexe.Masculin,
            FamilleId = famille.Id,
            ClasseId = classe.Id,
            AnneeScolaireId = anneeScolaire.Id,
            Statut = StatutEleve.Actif
        };
        context.Eleves.Add(eleve);

        // Type de frais
        var typeFrais = new TypeFrais
        {
            Id = Guid.NewGuid(),
            Nom = "Scolarité",
            Description = "Frais de scolarité annuelle",
            EstRecurrent = true
        };
        context.TypesFrais.Add(typeFrais);

        context.SaveChanges();
    }

    public static void Destroy(ApplicationDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Dispose();
    }
}