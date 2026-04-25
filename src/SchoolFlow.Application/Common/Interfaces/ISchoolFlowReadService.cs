using SchoolFlow.Shared.Dtos;
 
namespace SchoolFlow.Application.Common.Interfaces;
 
/// <summary>
/// Service de lecture via Dapper — remplace les Query Handlers EF Core lourds.
/// Toutes les méthodes ici font de la lecture pure, sans tracking EF.
/// </summary>
public interface ISchoolFlowReadService
{
    Task<DashboardStatsReadDto> GetDashboardStatsAsync(Guid ecoleId, CancellationToken ct = default);
 
    Task<PagedResultDto<FamilleDto>> GetFamillesAsync(
        Guid ecoleId, string? search, int page, int pageSize, CancellationToken ct = default);
 
    Task<IReadOnlyList<PaiementDto>> GetHistoriquePaiementsFamilleAsync(
        Guid ecoleId, Guid familleId, CancellationToken ct = default);
 
    Task<IReadOnlyList<FamilleImpayeeDto>> GetTopImpayesAsync(
        Guid ecoleId, int top = 10, CancellationToken ct = default);
 
    Task<IReadOnlyList<EleveListDto>> GetElevesParClasseAsync(
        Guid ecoleId, Guid classeId, CancellationToken ct = default);

    Task<IReadOnlyList<BulletinResumeDto>> GetBulletinsClasseAsync(
        Guid ecoleId, Guid classeId, Guid periodeId, CancellationToken ct = default);

    Task<EmploiDuTempsClasseDto> GetEmploiDuTempsClasseAsync(
        Guid ecoleId, Guid classeId, Guid anneeScolaireId, CancellationToken ct = default);
}