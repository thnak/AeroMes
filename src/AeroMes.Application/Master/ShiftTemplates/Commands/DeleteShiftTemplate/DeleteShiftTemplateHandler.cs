using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ShiftTemplates.Commands.DeleteShiftTemplate;

public class DeleteShiftTemplateHandler(
    IShiftTemplateRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteShiftTemplateCommand>
{
    public async Task HandleAsync(DeleteShiftTemplateCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct)
            ?? throw new EntityNotFoundException("ShiftTemplate", cmd.Code);

        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
    }
}
