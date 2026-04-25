namespace SchoolFlow.Domain.Entities;

public class CreneauHoraire : TenantEntity
{
    public Guid ClasseId { get; set; }
    public Guid MatiereId { get; set; }
    public Guid AnneeScolaireId { get; set; }
    public Guid? EnseignantId { get; set; }

    public JourSemaine Jour { get; set; }
    public TimeOnly HeureDebut { get; set; }
    public TimeOnly HeureFin { get; set; }
    public string? Salle { get; set; }
    public string? Remarque { get; set; }

    // Navigation
    public Classe Classe { get; set; } = null!;
    public Matiere Matiere { get; set; } = null!;
    public Enseignant? Enseignant { get; set; }

    public bool ChevaucheAvec(CreneauHoraire autre) =>
        Jour == autre.Jour &&
        !(HeureFin <= autre.HeureDebut || HeureDebut >= autre.HeureFin);
}

public enum JourSemaine
{
    Lundi = 1,
    Mardi = 2,
    Mercredi = 3,
    Jeudi = 4,
    Vendredi = 5,
    Samedi = 6
}
