using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Paiements.Commands;

public record EnregistrerPaiementCommand : IRequest<Result<string>>
{
    public Guid FamilleId { get; init; }
    public decimal MontantTotal { get; init; }
    public DateTime DatePaiement { get; init; }
    public ModePaiement ModePaiement { get; init; }
    public string? Reference { get; init; }
    public List<VentilationDto> Ventilations { get; init; } = new();
    public Guid EnregistrePar { get; init; }
}

public record VentilationDto(Guid FraisId, decimal MontantVentile);

public class EnregistrerPaiementCommandValidator : AbstractValidator<EnregistrerPaiementCommand>
{
    public EnregistrerPaiementCommandValidator()
    {
        RuleFor(x => x.MontantTotal)
            .GreaterThan(0).WithMessage("Le montant doit être supérieur à 0");

        RuleFor(x => x.DatePaiement)
            .LessThanOrEqualTo(DateTime.Today).WithMessage("La date ne peut pas être dans le futur");

        RuleFor(x => x.Ventilations)
            .NotEmpty().WithMessage("Au moins une ventilation est requise")
            .Must((cmd, ventilations) => ventilations.Sum(v => v.MontantVentile) == cmd.MontantTotal)
            .WithMessage("La somme des ventilations doit égaler le montant total");
    }
}

public class EnregistrerPaiementCommandHandler : IRequestHandler<EnregistrerPaiementCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public EnregistrerPaiementCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(EnregistrerPaiementCommand request, CancellationToken ct)
    {
        // Générer numéro paiement
        var numeroPaiement = await GenerateNumeroPaiementAsync(ct);

        var paiement = new Paiement
        {
            NumeroPaiement = numeroPaiement,
            FamilleId = request.FamilleId,
            MontantTotal = request.MontantTotal,
            DatePaiement = request.DatePaiement,
            ModePaiement = request.ModePaiement,
            Reference = request.Reference,
            EnregistrePar = request.EnregistrePar
        };

        _context.Paiements.Add(paiement);

        // Créer ventilations
        foreach (var ventDto in request.Ventilations)
        {
            var frais = await _context.Frais.FindAsync(new object[] { ventDto.FraisId }, ct);
            if (frais == null)
                return Result<string>.Failure($"Frais {ventDto.FraisId} introuvable");

            var ventilation = new VentilationPaiement
            {
                PaiementId = paiement.Id,
                FraisId = ventDto.FraisId,
                MontantVentile = ventDto.MontantVentile
            };

            _context.VentilationsPaiement.Add(ventilation);

            // Mettre à jour montant payé du frais
            frais.MontantPaye += ventDto.MontantVentile;
        }

        await _context.SaveChangesAsync(ct);

        return Result<string>.Success(numeroPaiement);
    }

    private async Task<string> GenerateNumeroPaiementAsync(CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var lastPaiement = await _context.Paiements
            .Where(p => p.NumeroPaiement.StartsWith($"PAY-{year}"))
            .OrderByDescending(p => p.NumeroPaiement)
            .Select(p => p.NumeroPaiement)
            .FirstOrDefaultAsync(ct);

        int sequence = 1;
        if (lastPaiement != null)
        {
            var lastSeq = lastPaiement.Split('-').Last();
            if (int.TryParse(lastSeq, out int num))
                sequence = num + 1;
        }

        return $"PAY-{year}-{sequence:D5}";
    }
}