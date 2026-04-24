using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.AnneeScolaires.Commands;

public record CreateAnneeScolaireCommand(
    string Libelle,
    DateTime DateDebut,
    DateTime DateFin,
    TypePeriode TypePeriode,
    bool ActiverImmediatement = false
) : IRequest<Result<Guid>>;
