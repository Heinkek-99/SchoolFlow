namespace SchoolFlow.Domain.Entities;

/// <summary>
/// Bulletin de notes par élève, période et année scolaire.
/// Généré automatiquement depuis les notes publiées.
/// </summary>
public class Bulletin : TenantEntity
{
    public Guid EleveId { get; set; }
    public Guid ClasseId { get; set; }
    public Guid PeriodeId { get; set; }
    public Guid AnneeScolaireId { get; set; }

    public decimal MoyenneGenerale { get; set; }
    public int RangClasse { get; set; }
    public int EffectifClasse { get; set; }
    public string? AppreciationProfesseur { get; set; }
    public string? AppreciationDirecteur { get; set; }
    public bool EstPublie { get; set; } = false;

    public string Appreciation => MoyenneGenerale switch
    {
        >= 16 => "Très Bien",
        >= 14 => "Bien",
        >= 12 => "Assez Bien",
        >= 10 => "Passable",
        >= 8  => "Insuffisant",
        _     => "Médiocre"
    };

    public string MentionBac => MoyenneGenerale switch
    {
        >= 16 => "Très Honorable",
        >= 14 => "Honorable",
        >= 12 => "Assez Bien",
        >= 10 => "Passable",
        _     => "Refusé"
    };

    // Navigation
    public Eleve Eleve { get; set; } = null!;
    public Classe Classe { get; set; } = null!;
    public Periode Periode { get; set; } = null!;
    public ICollection<LigneBulletin> Lignes { get; set; } = new List<LigneBulletin>();

    public void Publier()
    {
        EstPublie = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>Ligne du bulletin : 1 ligne = 1 matière avec sa moyenne pondérée</summary>
public class LigneBulletin : TenantEntity
{
    public Guid BulletinId { get; set; }
    public Guid MatiereId { get; set; }

    public decimal MoyenneMatiere { get; set; }
    public decimal Coefficient { get; set; }
    public decimal MoyennePonderee { get; set; }
    public string? AppreciationEnseignant { get; set; }
    public decimal? MoyenneClasse { get; set; }

    public Bulletin Bulletin { get; set; } = null!;
    public Matiere Matiere { get; set; } = null!;
}
