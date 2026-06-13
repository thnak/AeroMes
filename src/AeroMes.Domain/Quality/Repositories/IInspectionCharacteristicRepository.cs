namespace AeroMes.Domain.Quality.Repositories;

public interface IInspectionCharacteristicRepository
{
    Task<InspectionCharacteristic?> GetByIdAsync(int charId, CancellationToken ct = default);
    Task<IReadOnlyList<InspectionCharacteristic>> GetByPlanIdAsync(int planId, CancellationToken ct = default);
    void Remove(InspectionCharacteristic characteristic);
}
