using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(Utilisateur user);
}