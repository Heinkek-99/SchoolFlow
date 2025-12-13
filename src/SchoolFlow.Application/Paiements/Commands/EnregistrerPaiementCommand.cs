using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Paiements.Commands;

public record VentilationInput(
    Guid EleveId,
    decimal Montant,
    string? Remarque
);

public record EnregistrerPaiementCommand : IRequest<Result<string>>
{
    public Guid FamilleId { get; init; }
    public decimal MontantTotal { get; init; }
    public DateTime DatePaiement { get; init; }
    public ModePaiement ModePaiement { get; init; }
    public string? Reference { get; init; }
    public string? Commentaire { get; init; }
    public List<VentilationInput> Ventilations { get; init; } = new();
    public Guid EnregistrePar { get; init; }
}
