using MediatR;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Familles.Commands;

public class UpdateFamilleCommandHandler : IRequestHandler<UpdateFamilleCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public UpdateFamilleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateFamilleCommand request, CancellationToken ct)
    {
        var famille = await _context.Familles.FindAsync(new object[] { request.Id }, ct);
        
        if (famille == null)
            return Result<bool>.Failure("Famille introuvable");

        if (!string.IsNullOrEmpty(request.TelephonePere))
            famille.TelephonePere = request.TelephonePere;
        if (!string.IsNullOrEmpty(request.EmailPere))
            famille.EmailPere = request.EmailPere;
        if (!string.IsNullOrEmpty(request.TelephoneMere))
            famille.TelephoneMere = request.TelephoneMere;
        if (!string.IsNullOrEmpty(request.Adresse))
            famille.Adresse = request.Adresse;
        if (!string.IsNullOrEmpty(request.TelephonePrincipal))
            famille.TelephonePrincipal = request.TelephonePrincipal;

        famille.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}