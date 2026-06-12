using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Jobs.Queries.GetJobDetail;

public class GetJobDetailHandler(
    IJobRepository jobRepo,
    IProductionLogRepository logRepo)
    : IQueryHandler<GetJobDetailQuery, JobDetailDto>
{
    public async Task<JobDetailDto> HandleAsync(GetJobDetailQuery q, CancellationToken ct)
    {
        var job = await jobRepo.GetByIdAsync(q.Id, ct)
            ?? throw new EntityNotFoundException("Job", q.Id);

        var logs = await logRepo.GetByJobIdAsync(q.Id, ct);

        return new JobDetailDto(
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
                l.Notes)).ToList());
    }
}
