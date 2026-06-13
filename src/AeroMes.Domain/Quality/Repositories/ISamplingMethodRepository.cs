namespace AeroMes.Domain.Quality.Repositories;

public record SamplingVolumeRangeDto(int RangeID, int MinQty, int MaxQty, decimal SampleSizeOrRatio, int MaxDefects);

public record SamplingMethodDto(
    int SamplingMethodID,
    string Code,
    string Name,
    string? Notes,
    string Status,
    string SamplingType,
    decimal? SampleQuantity,
    int MaxDefects,
    IReadOnlyList<SamplingVolumeRangeDto> VolumeRanges);

public interface ISamplingMethodRepository
{
    Task AddAsync(SamplingMethod method, CancellationToken ct = default);
    Task<SamplingMethod?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<SamplingMethodDto>> GetListAsync(bool? activeOnly, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
