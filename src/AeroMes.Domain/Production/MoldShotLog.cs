using AeroMes.Domain.Common;

namespace AeroMes.Domain.Production;

public class MoldShotLog : Entity
{
    public long LogID { get; private set; }
    public string MoldCode { get; private set; } = string.Empty;
    public long JobID { get; private set; }
    public long ShotsThisJob { get; private set; }
    public DateTime RecordedAt { get; private set; }

    private MoldShotLog() { }

    public static MoldShotLog Create(string moldCode, long jobId, long shotsThisJob)
        => new()
        {
            MoldCode = moldCode.Trim().ToUpperInvariant(),
            JobID = jobId,
            ShotsThisJob = shotsThisJob,
            RecordedAt = DateTime.UtcNow,
        };
}
