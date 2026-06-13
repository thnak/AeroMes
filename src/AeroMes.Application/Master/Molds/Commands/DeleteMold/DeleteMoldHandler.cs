using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Molds.Commands.DeleteMold;

public class DeleteMoldHandler(
    IMoldRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteMoldCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteMoldCommand cmd, CancellationToken ct)
    {
        var mold = await repo.GetByCodeAsync(cmd.MoldCode, ct);
        if (mold is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.MoldCode}' was not found.");

        if (mold.CurrentMachineCode is not null)
            throw new DomainException(
                $"Phải tháo khuôn '{mold.MoldCode}' khỏi máy trước khi xóa.");

        mold.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
