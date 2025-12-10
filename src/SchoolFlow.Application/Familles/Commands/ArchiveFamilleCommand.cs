using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;

namespace SchoolFlow.Application.Familles.Commands;

public record ArchiveFamilleCommand(Guid Id) : IRequest<Result<bool>>;

public class ArchiveFamilleCommandHandler : IRequestHandler<ArchiveFamilleCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public ArchiveFamilleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(ArchiveFamilleCommand request, CancellationToken ct)
    {
        var famille = await _context.Familles
            .Include(f => f.Eleves)
            .FirstOrDefaultAsync(f => f.Id == request.Id, ct);
        
        if (famille == null)
            return Result<bool>.Failure("Famille introuvable");

        if (famille.Eleves.Any(e => !e.IsArchived))
            return Result<bool>.Failure("Impossible d'archiver une famille avec des élèves actifs");

        famille.IsArchived = true;
        famille.ArchivedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}