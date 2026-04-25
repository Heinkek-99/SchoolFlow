using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Disciplines.Handlers;

public record GetDisciplinesEleveQuery(Guid EleveId) : IRequest<Result<List<DisciplineDto>>>;

public class GetDisciplinesEleveQueryHandler
    : IRequestHandler<GetDisciplinesEleveQuery, Result<List<DisciplineDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetDisciplinesEleveQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<List<DisciplineDto>>> Handle(
        GetDisciplinesEleveQuery request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;

        var disciplines = await _context.Disciplines
            .Include(d => d.SignaleParUtilisateur)
            .Where(d => d.EleveId == request.EleveId && d.EcoleId == ecoleId)
            .OrderByDescending(d => d.DateDiscipline)
            .ToListAsync(ct);

        var dtos = disciplines.Select(d => new DisciplineDto(
            d.Id,
            d.Type.ToString(),
            d.Motif,
            d.DateDiscipline,
            d.Mesure,
            d.NotifieParent,
            d.SignaleParUtilisateur?.NomComplet
        )).ToList();

        return Result<List<DisciplineDto>>.Success(dtos);
    }
}

public record CreateDisciplineCommand(
    Guid EleveId, Guid AnneeScolaireId,
    string Type, string Motif, DateTime DateDiscipline,
    string? Mesure, bool NotifieParent
) : IRequest<Result<Guid>>;

public class CreateDisciplineCommandHandler
    : IRequestHandler<CreateDisciplineCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateDisciplineCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateDisciplineCommand request, CancellationToken ct)
    {
        var ecoleId = _currentUser.EcoleId;
        if (ecoleId == Guid.Empty)
            return Result<Guid>.Failure("Contexte école manquant.");

        if (!Enum.TryParse<TypeDiscipline>(request.Type, out var type))
            return Result<Guid>.Failure("Type de discipline invalide.");

        var discipline = new Discipline
        {
            EcoleId = ecoleId,
            EleveId = request.EleveId,
            AnneeScolaireId = request.AnneeScolaireId,
            SignalePar = _currentUser.UserId,
            Type = type,
            Motif = request.Motif,
            DateDiscipline = request.DateDiscipline == default
                ? DateTime.UtcNow
                : request.DateDiscipline.ToUniversalTime(),
            Mesure = request.Mesure,
            NotifieParent = request.NotifieParent
        };

        _context.Disciplines.Add(discipline);
        await _context.SaveChangesAsync(ct);
        return Result<Guid>.Success(discipline.Id);
    }
}
