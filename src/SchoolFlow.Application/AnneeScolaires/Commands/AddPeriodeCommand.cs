using MediatR;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.AnneeScolaires.Commands;

public record AddPeriodeCommand(
    Guid AnneeScolaireId,
    string Libelle,
    TypePeriode Type,
    DateTime DateDebut,
    DateTime DateFin,
    int Numero
) : IRequest<Result<Guid>>;
