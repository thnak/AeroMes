using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Molds.Commands.DeleteMold;

public class DeleteMoldHandler(
    IMoldRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteMoldCommand>
{
    public async Task HandleAsync(DeleteMoldCommand cmd, CancellationToken ct)
    {
        var mold = await repo.GetByCodeAsync(cmd.MoldCode, ct)
            ?? throw new EntityNotFoundException(nameof(Mold), cmd.MoldCode);

        if (mold.CurrentMachineCode is not null)
            throw new DomainException(
                $"Phải tháo khuôn '{mold.MoldCode}' khỏi máy trước khi xóa.");

        mold.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
    }
}
