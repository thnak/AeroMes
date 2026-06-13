namespace AeroMes.Domain.Quality.Repositories;

public record AQLInspectionDto(
    int AQLInspectionID,
    int WOID,
    string AQLLevel,
    string InspectionLevel,
    int LotSize,
    int SampleSize,
    int AcceptanceNumber,
    int RejectionNumber,
    int DefectsFound,
    string Decision,
    string InspectorID,
    DateTime InspectedAt,
    string? Notes);

public interface IAQLInspectionRepository
{
    Task AddAsync(AQLInspection inspection, CancellationToken ct = default);
    Task<IReadOnlyList<AQLInspectionDto>> GetByWOAsync(int woid, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
