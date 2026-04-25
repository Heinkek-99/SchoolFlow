using MediatR;
using SchoolFlow.Application.Common.Models;

namespace SchoolFlow.Application.AnneeScolaires.Commands;

public record UpdateAnneeScolaireCommand(
    Guid Id,
    string Libelle,
    DateTime DateDebut,
    DateTime DateFin
) : IRequest<Result<string>>;
