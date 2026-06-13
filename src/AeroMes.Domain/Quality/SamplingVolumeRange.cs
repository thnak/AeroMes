using AeroMes.Domain.Common;

namespace AeroMes.Domain.Quality;

public class SamplingVolumeRange : Entity
{
    public int RangeID { get; private set; }
    public int SamplingMethodID { get; private set; }
    public int MinQty { get; private set; }
    public int MaxQty { get; private set; }
    public decimal SampleSizeOrRatio { get; private set; }
    public int MaxDefects { get; private set; }

    private SamplingVolumeRange() { }

    internal static SamplingVolumeRange Create(
        int samplingMethodId, int minQty, int maxQty, decimal sampleSizeOrRatio, int maxDefects)
    {
        return new SamplingVolumeRange
        {
            SamplingMethodID = samplingMethodId,
            MinQty = minQty,
            MaxQty = maxQty,
            SampleSizeOrRatio = sampleSizeOrRatio,
            MaxDefects = maxDefects,
        };
    }
}
