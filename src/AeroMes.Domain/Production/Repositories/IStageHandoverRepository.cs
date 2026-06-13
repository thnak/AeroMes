namespace AeroMes.Domain.Production.Repositories;

public interface IStageHandoverRepository
{
    Task<StageHandoverForm?> GetByIdAsync(int formId, CancellationToken ct);
    Task<StageHandoverForm?> GetByIdWithLinesAsync(int formId, CancellationToken ct);
    Task<IReadOnlyList<StageHandoverForm>> GetByWorkOrderAsync(int workOrderId, CancellationToken ct);
    Task<IReadOnlyList<StageHandoverForm>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct);
    Task<string> GenerateFormNumberAsync(HandoverFormType formType, CancellationToken ct);
    Task AddAsync(StageHandoverForm form, CancellationToken ct);
}
