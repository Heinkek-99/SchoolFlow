using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Application.Familles.Queries;

public record GetAllFamillesQuery(int Page = 1, int PageSize = 50) : IRequest<Result<List<FamilleDto>>>;

