using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.WorkShifts.Commands.CreateWorkShift;

public class CreateWorkShiftHandler(
    IWorkShiftRepository repo,
    IUnitOfWork uow) : ICommandHandler<CreateWorkShiftCommand, int>
{
    public async Task<int> HandleAsync(CreateWorkShiftCommand cmd, CancellationToken ct)
    {
        var entity = WorkShift.Create(
            cmd.Code, cmd.Name, cmd.StartTime, cmd.EndTime,
            cmd.Breaks.Select(b => (b.BreakStart, b.BreakEnd)),
            cmd.CreatedBy);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.WorkShiftId;
    }
}
