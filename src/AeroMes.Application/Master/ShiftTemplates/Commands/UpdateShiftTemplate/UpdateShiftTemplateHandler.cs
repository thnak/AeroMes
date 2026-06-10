using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ShiftTemplates.Commands.UpdateShiftTemplate;

public class UpdateShiftTemplateHandler(
    IShiftTemplateRepository repo,
    IUnitOfWork uow) : ICommandHandler<UpdateShiftTemplateCommand>
{
    public async Task HandleAsync(UpdateShiftTemplateCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException("ShiftTemplate", cmd.Code);

        entity.UpdateDetails(cmd.Name, cmd.StartTime, cmd.EndTime,
            cmd.IsNightShift, cmd.ValidDays, cmd.WorkCenterId, cmd.IsActive, cmd.UpdatedBy);
        await uow.SaveChangesAsync(ct);
    }
}
