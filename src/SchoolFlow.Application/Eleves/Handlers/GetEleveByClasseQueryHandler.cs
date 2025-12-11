namespace SchoolFlow.Application.Eleves.Handlers;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Common.Models;
using SchoolFlow.Application.Eleves.Queries;
using SchoolFlow.Shared.Dtos;

public class GetEleveByClasseQueryHandler : IRequestHandler<GetEleveByClasseQuery, Result<List<EleveDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetEleveByClasseQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Result<List<EleveDto>>> Handle(GetEleveByClasseQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
