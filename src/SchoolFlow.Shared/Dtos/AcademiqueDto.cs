namespace SchoolFlow.Shared.Dtos;

public record MatiereDto(
    Guid Id, string Code, string Libelle,
    decimal Coefficient, string SousSysteme, bool IsActive
);

public record EnseignantDto(
    Guid Id, string NomComplet, string Telephone,
    string? Specialite, string? Grade, bool IsActive,
    List<string> Matieres
);

public record EvaluationDto(
    Guid Id, string TypeEvaluation, DateTime DateEvaluation,
    string Matiere, string Classe, string Periode,
    decimal NoteSur, bool EstPubliee, int NombreNotesSaisies
);

public record NoteDto(
    Guid Id, string EleveNom, string EleveMatricule,
    decimal Valeur, decimal NoteSur, decimal ValeurSur20,
    string Appreciation, string? Commentaire
);

public record BulletinResumeDto(
    Guid Id, string EleveNom, string EleveMatricule,
    decimal MoyenneGenerale, int RangClasse, int EffectifClasse,
    string Appreciation, bool EstPublie
);

public record BulletinDetailDto(
    Guid Id,
    string EleveNom, string EleveMatricule,
    string Classe, string Periode, string AnneeScolaire,
    decimal MoyenneGenerale, int RangClasse, int EffectifClasse,
    string Appreciation, bool EstPublie,
    List<LigneBulletinDto> Matieres,
    string? AppreciationProfesseur,
    string? AppreciationDirecteur
);

public record LigneBulletinDto(
    string CodeMatiere, string LibelleMatiere,
    decimal Coefficient, decimal MoyenneMatiere,
    decimal MoyennePonderee, decimal? MoyenneClasse,
    string Appreciation, string? AppreciationEnseignant
);

public record CreneauDto(
    Guid Id, string Jour, string HeureDebut, string HeureFin,
    string Matiere, string? Enseignant, string? Salle
);

public record EmploiDuTempsClasseDto(
    Guid ClasseId, string NomClasse,
    Dictionary<string, List<CreneauDto>> PlanningParJour
);

public record DisciplineDto(
    Guid Id, string TypeDiscipline, string Motif,
    DateTime Date, string? Mesure, bool NotifieParent,
    string? SignalePar
);

public record ExamenDto(
    Guid Id, string Nom, string Type,
    DateTime DateDebut, DateTime DateFin,
    int NombreInscrits
);

public record ResultatExamenDto(
    Guid EleveId, string EleveNom, string Matricule,
    string? NumeroCandidat, bool Admis,
    decimal? MoyenneExamen, string? Mention
);
