namespace AeroMes.Domain.Production.Repositories;

public record MoldAssignmentDto(
    int AssignmentID,
    string MoldCode,
    string MachineCode,
    int WOID,
    DateTime MountedAt,
    DateTime? UnmountedAt,
    string MountedBy);

public interface IMoldAssignmentRepository
{
    Task AddAsync(MoldAssignment assignment, CancellationToken ct = default);
    Task<MoldAssignment?> GetActiveMountAsync(string moldCode, CancellationToken ct = default);
    Task<IReadOnlyList<MoldAssignmentDto>> GetHistoryAsync(
        string moldCode, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
    Task AddShotLogAsync(MoldShotLog shotLog, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
