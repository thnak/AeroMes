using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ShiftTemplates.Commands.CreateShiftTemplate;

public class CreateShiftTemplateHandler(
    IShiftTemplateRepository repo,
    IUnitOfWork uow) : ICommandHandler<CreateShiftTemplateCommand, string>
{
    public async Task<string> HandleAsync(CreateShiftTemplateCommand cmd, CancellationToken ct)
    {
        var entity = ShiftTemplate.Create(
            cmd.Code, cmd.Name, cmd.StartTime, cmd.EndTime,
            cmd.IsNightShift, cmd.ValidDays, cmd.WorkCenterId, cmd.CreatedBy);
        await repo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        return entity.ShiftCode;
    }
}
