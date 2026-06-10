using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Jobs.Commands.FinishJob;

public class FinishJobHandler(
    IJobRepository jobRepo,
    IUnitOfWork uow)
    : ICommandHandler<FinishJobCommand, FinishJobResult>
{
    public async Task<FinishJobResult> HandleAsync(FinishJobCommand cmd, CancellationToken ct)
    {
        var job = await jobRepo.GetByIdAsync(cmd.JobId, ct)
            ?? throw new EntityNotFoundException(nameof(Job), cmd.JobId);

        job.Finish(cmd.EndTime);
        await uow.SaveChangesAsync(ct);

        return new FinishJobResult(job.JobID, job.Status.ToString().ToUpperInvariant(), job.EndTime!.Value);
    }
}
