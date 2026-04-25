using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Matieres.Handlers;

// ── QUERIES ──────────────────────────────────────────────────────────────────

public record GetAllMatieresQuery(string? SousSysteme) : IRequest<Result<List<MatiereDto>>>;

public class GetAllMatieresQueryHandler
    : IRequestHandler<GetAllMatieresQuery, Result<List<MatiereDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAllMatieresQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<MatiereDto>>> Handle(
        GetAllMatieresQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        if (ecoleId == Guid.Empty)
            return Result<List<MatiereDto>>.Failure("Contexte école manquant.");

        var query = _context.Matieres.Where(m => m.EcoleId == ecoleId);

        if (!string.IsNullOrWhiteSpace(request.SousSysteme) &&
            Enum.TryParse<SousSysteme>(request.SousSysteme, out var ss))
            query = query.Where(m => m.SousSysteme == ss);

        var matieres = await query
            .OrderBy(m => m.Code)
            .Select(m => new MatiereDto(
                m.Id, m.Code, m.Libelle,
                m.Coefficient, m.SousSysteme.ToString(), m.IsActive))
            .ToListAsync(ct);

        return Result<List<MatiereDto>>.Success(matieres);
    }
}

// ── COMMANDS ─────────────────────────────────────────────────────────────────

public record CreateMatiereCommand(
    string Code, string Libelle, decimal Coefficient,
    string SousSysteme, string? Description
) : IRequest<Result<Guid>>;

public class CreateMatiereCommandHandler
    : IRequestHandler<CreateMatiereCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateMatiereCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateMatiereCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        if (ecoleId == Guid.Empty)
            return Result<Guid>.Failure("Contexte école manquant.");

        if (!Enum.TryParse<SousSysteme>(request.SousSysteme, out var ss))
            return Result<Guid>.Failure("SousSysteme invalide.");

        var existe = await _context.Matieres
            .AnyAsync(m => m.EcoleId == ecoleId &&
                           m.Code.ToLower() == request.Code.ToLower().Trim(), ct);
        if (existe)
            return Result<Guid>.Failure($"Une matière avec le code '{request.Code}' existe déjà.");

        var matiere = new Matiere
        {
            EcoleId = ecoleId,
            Code = request.Code.Trim().ToUpper(),
            Libelle = request.Libelle.Trim(),
            Coefficient = request.Coefficient,
            SousSysteme = ss,
            Description = request.Description,
            IsActive = true
        };

        _context.Matieres.Add(matiere);
        await _context.SaveChangesAsync(ct);
        return Result<Guid>.Success(matiere.Id);
    }
}

public record UpdateMatiereCommand(
    Guid Id, string Libelle, decimal Coefficient,
    string? Description, bool IsActive
) : IRequest<Result<bool>>;

public class UpdateMatiereCommandHandler
    : IRequestHandler<UpdateMatiereCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateMatiereCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(UpdateMatiereCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        var matiere = await _context.Matieres
            .FirstOrDefaultAsync(m => m.Id == request.Id && m.EcoleId == ecoleId, ct);

        if (matiere is null)
            return Result<bool>.Failure("Matière introuvable.");

        matiere.Libelle = request.Libelle.Trim();
        matiere.Coefficient = request.Coefficient;
        matiere.Description = request.Description;
        matiere.IsActive = request.IsActive;
        matiere.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}

public record ArchiveMatiereCommand(Guid Id) : IRequest<Result<bool>>;

public class ArchiveMatiereCommandHandler
    : IRequestHandler<ArchiveMatiereCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ArchiveMatiereCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(ArchiveMatiereCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        var matiere = await _context.Matieres
            .FirstOrDefaultAsync(m => m.Id == request.Id && m.EcoleId == ecoleId, ct);

        if (matiere is null)
            return Result<bool>.Failure("Matière introuvable.");

        matiere.IsArchived = true;
        matiere.ArchivedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
