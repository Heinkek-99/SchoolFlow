using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Eleves.Commands;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Eleves.Handlers;

public class ArchiveEleveCommandHandler : IRequestHandler<ArchiveEleveCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public ArchiveEleveCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(ArchiveEleveCommand request, CancellationToken ct)
    {
        var eleve = await _context.Eleves
            .Include(e => e.Frais)
            .FirstOrDefaultAsync(e => e.Id == request.EleveId, ct);

        if (eleve == null)
            return Result<bool>.Failure("Élève introuvable");

        if (eleve.IsArchived)
            return Result<bool>.Failure("L'élève est déjà archivé");

        // Vérifier s'il y a des frais impayés
        var fraisImpayes = eleve.Frais
            .Where(f => !f.IsArchived && f.Solde > 0)
            .Sum(f => f.Solde);

        if (fraisImpayes > 0)
        {
            return Result<bool>.Failure(
                $"Impossible d'archiver l'élève. Solde impayé de {fraisImpayes:N0} FCFA"
            );
        }

        eleve.IsArchived = true;
        eleve.ArchivedAt = DateTime.UtcNow;
        eleve.ArchivedBy = request.ArchivedBy;
        eleve.ArchiveReason = $"{request.MotifArchivage}{(string.IsNullOrEmpty(request.Commentaire) ? "" : $" - {request.Commentaire}")}";
        eleve.Statut = StatutEleve.Radie;

        await _context.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}