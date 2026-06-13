using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Jobs.Queries.GetJobDetail;

public class GetJobDetailHandler(
    IJobRepository jobRepo,
    IProductionLogRepository logRepo)
    : IQueryHandler<GetJobDetailQuery, QueryResult<JobDetailDto>>
{
    public async Task<QueryResult<JobDetailDto>> HandleAsync(GetJobDetailQuery q, CancellationToken ct)
    {
        var job = await jobRepo.GetByIdAsync(q.Id, ct);
        if (job is null) return QueryResult<JobDetailDto>.NotFound($"Job '{q.Id}' was not found.");

        var logs = await logRepo.GetByJobIdAsync(q.Id, ct);

        return QueryResult<JobDetailDto>.Found(new JobDetailDto(
            job.JobID,
            job.WOID,
            job.MachineCode,
            job.ShiftCode,
            job.OperatorID,
            job.StartTime,
            job.EndTime,
            job.Status.ToString().ToUpperInvariant(),
            logs.Select(l => new ProductionLogDto(
                l.LogID,
                l.Timestamp,
                l.QtyOK,
                l.QtyNG,
                l.DeviceIP,
                l.Notes)).ToList()));
    }
}
