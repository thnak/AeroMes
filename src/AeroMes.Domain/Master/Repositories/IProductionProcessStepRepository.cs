namespace AeroMes.Domain.Master.Repositories;

public record ProductionProcessStepDto(
    int StepID, string Code, string Name, string? Description,
    string ApplicationScope, string? ProductGroupIdsJson, string? ProductIdsJson,
    bool IsActive, DateTime CreatedAt);

public interface IProductionProcessStepRepository
{
    Task<int> AddAsync(ProductionProcessStep step, CancellationToken ct);
    Task<ProductionProcessStep?> GetByIdAsync(int stepId, CancellationToken ct);
    Task<ProductionProcessStep?> GetByCodeAsync(string code, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    Task<bool> IsReferencedByProcessAsync(string code, CancellationToken ct);
    Task<(IReadOnlyList<ProductionProcessStepDto> Items, int Total)> GetListAsync(
        string? keyword, string? scope, bool? isActive, int page, int pageSize, CancellationToken ct);
    Task DeleteAsync(ProductionProcessStep step, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
