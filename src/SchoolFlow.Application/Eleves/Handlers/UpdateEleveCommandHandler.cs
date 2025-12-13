using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Eleves.Commands;

namespace SchoolFlow.Application.Eleves.Handlers;

public class UpdateEleveCommandHandler : IRequestHandler<UpdateEleveCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public UpdateEleveCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateEleveCommand request, CancellationToken ct)
    {
        var eleve = await _context.Eleves
            .FirstOrDefaultAsync(e => e.Id == request.Id, ct);

        if (eleve == null)
            return Result<bool>.Failure("Élève introuvable");

        // Mettre à jour uniquement les champs non-null
        if (request.Nom != null) eleve.Nom = request.Nom;
        if (request.Prenom != null) eleve.Prenom = request.Prenom;
        if (request.DateNaissance != null) eleve.DateNaissance = request.DateNaissance.Value;
        if (request.LieuNaissance != null) eleve.LieuNaissance = request.LieuNaissance;
        if (request.Sexe != null) eleve.Sexe = request.Sexe.Value;
        if (request.ClasseId != null) eleve.ClasseId = request.ClasseId.Value;
        if (request.PhotoPath != null) eleve.PhotoPath = request.PhotoPath;
        if (request.Nationalite != null) eleve.Nationalite = request.Nationalite;
        if (request.GroupeSanguin != null) eleve.GroupeSanguin = request.GroupeSanguin;
        if (request.Allergies != null) eleve.Allergies = request.Allergies;
        if (request.ContactUrgence != null) eleve.ContactUrgence = request.ContactUrgence;

        eleve.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}
