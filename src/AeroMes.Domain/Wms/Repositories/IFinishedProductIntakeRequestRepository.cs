namespace AeroMes.Domain.Wms.Repositories;

public interface IFinishedProductIntakeRequestRepository
{
    Task<IReadOnlyList<FinishedProductIntakeRequest>> GetAllAsync(
        IntakeRequestPurpose? intakePurpose,
        IntakeRequestStatus? status,
        int? productionOrderId,
        CancellationToken ct = default);
    Task<FinishedProductIntakeRequest?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<FinishedProductIntakeRequest?> GetByIdWithLinesAsync(int id, CancellationToken ct = default);
    Task<bool> RequestNumberExistsAsync(string number, CancellationToken ct = default);
    Task AddAsync(FinishedProductIntakeRequest request, CancellationToken ct = default);
}
