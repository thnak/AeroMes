using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Jobs.Queries.GetJobs;

public class GetJobsHandler(IJobRepository jobRepo)
    : IQueryHandler<GetJobsQuery, IReadOnlyList<JobDto>>
{
    public async Task<IReadOnlyList<JobDto>> HandleAsync(GetJobsQuery q, CancellationToken ct)
    {
        var status = Enum.TryParse<JobStatus>(q.Status, ignoreCase: true, out var parsed)
            ? parsed
            : (JobStatus?)null;

        var jobs = await jobRepo.GetFilteredAsync(q.WoId, q.MachineCode, status, q.From, q.To, ct);

        return jobs.Select(j => new JobDto(
            j.JobID,
            j.WOID,
            j.MachineCode,
            j.ShiftCode,
            j.OperatorID,
            j.StartTime,
            j.EndTime,
            j.Status.ToString().ToUpperInvariant())).ToList();
    }
}
