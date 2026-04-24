namespace SchoolFlow.Domain.Entities;

public class Classe : TenantEntity
{
    public string Code { get; set; } = string.Empty;          // "CM2-A", "Form1-A"
    public string Nom { get; set; } = string.Empty;           // "CM2", "Form 1"
    public Niveau Niveau { get; set; }
    public SousSysteme SousSysteme { get; set; } = SousSysteme.Francophone;
    public string? Section { get; set; }                       // "A", "B", "Sciences"
    public int CapaciteMax { get; set; } = 50;
    public decimal? FraisScolarite { get; set; }
    public StatutClasse Statut { get; set; } = StatutClasse.Active;
    public Guid AnneeScolaireId { get; set; }
    public Guid? TitulaireId { get; set; }

    // Calculés (ignorés par EF)
    public int EffectifActuel => Eleves.Count(e => !e.IsArchived);
    public bool EstPleine => EffectifActuel >= CapaciteMax;
    public int PlacesDisponibles => Math.Max(0, CapaciteMax - EffectifActuel);
    public string NomComplet => string.IsNullOrWhiteSpace(Section)
        ? Nom : $"{Nom} {Section}";

    // Navigation
    public AnneeScolaire AnneeScolaire { get; set; } = null!;
    public ICollection<Eleve> Eleves { get; set; } = new List<Eleve>();

    public static Classe Creer(
        string code, string nom, Niveau niveau,
        SousSysteme sousSysteme, string? section, int capaciteMax,
        Guid anneeScolaireId, Guid ecoleId, Guid? titulaireId = null)
    {
        return new Classe
        {
            Code = code.Trim(),
            Nom = nom.Trim(),
            Niveau = niveau,
            SousSysteme = sousSysteme,
            Section = section?.Trim(),
            CapaciteMax = capaciteMax,
            AnneeScolaireId = anneeScolaireId,
            EcoleId = ecoleId,
            TitulaireId = titulaireId,
            Statut = StatutClasse.Active
        };
    }

    public void MettreAJour(
        string nom, Niveau niveau,
        SousSysteme sousSysteme, string? section,
        int capaciteMax, Guid? titulaireId)
    {
        Nom = nom.Trim();
        Niveau = niveau;
        SousSysteme = sousSysteme;
        Section = section?.Trim();
        CapaciteMax = capaciteMax;
        TitulaireId = titulaireId;
    }

    public void Archiver() => Statut = StatutClasse.Archivee;
}

public enum SousSysteme { Francophone = 1, Anglophone = 2, Bilingue = 3 }
public enum StatutClasse { Active = 1, Inactive = 2, Archivee = 3 }

public enum Niveau
{
    // Francophone
    Maternelle = 1,
    SIL = 2,
    CP = 3,
    CE1 = 4,
    CE2 = 5,
    CM1 = 6,
    CM2 = 7,
    Sixieme = 8,
    Cinquieme = 9,
    Quatrieme = 10,
    Troisieme = 11,
    Seconde = 12,
    Premiere = 13,
    Terminale = 14,
    // Anglophone
    Nursery = 20,
    Primary1 = 21,
    Primary2 = 22,
    Primary3 = 23,
    Primary4 = 24,
    Primary5 = 25,
    Primary6 = 26,
    Form1 = 27,
    Form2 = 28,
    Form3 = 29,
    Form4 = 30,
    Form5 = 31,
    LowerSixth = 32,
    UpperSixth = 33
}
