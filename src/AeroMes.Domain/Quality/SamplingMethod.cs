using AeroMes.Domain.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Domain.Quality;

public enum SamplingType
{
    FixedSize,
    RatioPercentage,
    VolumeRange,
    RatioWithinVolumeRange
}

public enum SamplingMethodStatus { Active, Discontinued }

public class SamplingMethod : AuditableEntity
{
    public int SamplingMethodID { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public SamplingMethodStatus Status { get; private set; } = SamplingMethodStatus.Active;
    public SamplingType SamplingType { get; private set; }
    public decimal? SampleQuantity { get; private set; }
    public int MaxDefects { get; private set; }

    private readonly List<SamplingVolumeRange> _volumeRanges = [];
    public IReadOnlyList<SamplingVolumeRange> VolumeRanges => _volumeRanges.AsReadOnly();

    private SamplingMethod() { }

    public static SamplingMethod Create(
        string code, string name, SamplingType samplingType,
        decimal? sampleQuantity, int maxDefects, string? notes, string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Mã phương pháp không được để trống.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Tên phương pháp không được để trống.");
        if (maxDefects < 0)
            throw new DomainException("Số lỗi tối đa không thể âm.");

        return new SamplingMethod
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            SamplingType = samplingType,
            SampleQuantity = sampleQuantity,
            MaxDefects = maxDefects,
            Notes = notes?.Trim(),
            Status = SamplingMethodStatus.Active,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateDetails(
        string name, SamplingType samplingType,
        decimal? sampleQuantity, int maxDefects, string? notes,
        SamplingMethodStatus status, string? updatedBy)
    {
        Name = name.Trim();
        SamplingType = samplingType;
        SampleQuantity = sampleQuantity;
        MaxDefects = maxDefects;
        Notes = notes?.Trim();
        Status = status;
        Touch(updatedBy);
    }

    public void AddVolumeRange(int minQty, int maxQty, decimal sampleSizeOrRatio, int rangeMaxDefects, string? updatedBy)
    {
        if (SamplingType is not (SamplingType.VolumeRange or SamplingType.RatioWithinVolumeRange))
            throw new DomainException("Chỉ phương pháp VolumeRange / RatioWithinVolumeRange mới có dải khối lượng.");
        if (minQty < 0 || maxQty <= minQty)
            throw new DomainException("Khoảng khối lượng không hợp lệ.");

        _volumeRanges.Add(SamplingVolumeRange.Create(SamplingMethodID, minQty, maxQty, sampleSizeOrRatio, rangeMaxDefects));
        Touch(updatedBy);
    }

    public void ClearVolumeRanges(string? updatedBy)
    {
        _volumeRanges.Clear();
        Touch(updatedBy);
    }

    public void Discontinue(string? updatedBy)
    {
        Status = SamplingMethodStatus.Discontinued;
        Touch(updatedBy);
    }
}
