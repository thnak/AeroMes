namespace AeroMes.Domain.Master.Repositories;

public record ProductionProcessStageDto(
    int StageID, int SortOrder, string? ProcessStepCode,
    string CapacityType, string CapacityIdsJson,
    decimal PlannedTimeSeconds, string PlannedTimeSource,
    int TimeOffsetDays, bool IsPrimaryStage);

public record ProductionProcessListDto(
    int ProcessID, string Code, string Name, string ProcessType,
    DateOnly EffectiveDate, string ApplicationScope,
    bool IsForPlanningOnly, bool IsActive, DateTime CreatedAt, int StageCount);

public record ProductionProcessDetailDto(
    int ProcessID, string Code, string Name, string ProcessType,
    DateOnly EffectiveDate, string ApplicationScope,
    string? ProductGroupIdsJson, string? ProductIdsJson,
    bool IsForPlanningOnly, bool IsActive, DateTime CreatedAt,
    IReadOnlyList<ProductionProcessStageDto> Stages);

public interface IProductionProcessRepository
{
    Task<int> AddAsync(ProductionProcess process, CancellationToken ct);
    Task<ProductionProcess?> GetByIdAsync(int processId, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    Task<bool> IsReferencedByWorkOrderAsync(int processId, CancellationToken ct);
    Task<(IReadOnlyList<ProductionProcessListDto> Items, int Total)> GetListAsync(
        string? keyword, string? processType, bool? isActive, int page, int pageSize, CancellationToken ct);
    Task<ProductionProcessDetailDto?> GetDetailAsync(int processId, CancellationToken ct);
    Task DeleteAsync(ProductionProcess process, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
