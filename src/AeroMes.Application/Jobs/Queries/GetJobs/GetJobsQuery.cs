using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Jobs.Queries.GetJobs;

public record GetJobsQuery(
    int? WoId,
    string? MachineCode,
    string? Status,
    DateTime? From,
    DateTime? To) : IQuery<IReadOnlyList<JobDto>>;

public record JobDto(
    long JobID,
    int WOID,
    string MachineCode,
    string ShiftCode,
    string OperatorID,
    DateTime StartTime,
    DateTime? EndTime,
    string Status);
