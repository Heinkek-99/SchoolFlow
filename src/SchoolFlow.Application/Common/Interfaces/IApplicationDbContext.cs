using Microsoft.EntityFrameworkCore;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Utilisateur> Utilisateurs { get; }
    DbSet<Famille> Familles { get; }
    DbSet<Eleve> Eleves { get; }
    DbSet<Classe> Classes { get; }
    DbSet<AnneeScolaire> AnneeScolaires { get; }
    DbSet<Periode> Periodes { get; }
    DbSet<TypeFrais> TypeFrais { get; }
    DbSet<Frais> Frais { get; }
    DbSet<Paiement> Paiements { get; }
    DbSet<VentilationPaiement> VentilationsPaiement { get; }
    DbSet<Note> Notes { get; }
    DbSet<Matiere> Matieres { get; }
    DbSet<Enseignant> Enseignants { get; }
    DbSet<MatiereEnseignant> MatiereEnseignants { get; }
    DbSet<EvaluationPlanifiee> Evaluations { get; }
    DbSet<Bulletin> Bulletins { get; }
    DbSet<LigneBulletin> LignesBulletin { get; }
    DbSet<CreneauHoraire> CreneauxHoraires { get; }
    DbSet<Discipline> Disciplines { get; }
    DbSet<Examen> Examens { get; }
    DbSet<InscriptionExamen> InscriptionsExamen { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Ecole> Ecoles { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }


    Task<int> SaveChangesAsync(CancellationToken ct = default);
}