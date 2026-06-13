using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.DowntimeReasonCodes.Commands.DeleteDowntimeReasonCode;

public class DeleteDowntimeReasonCodeHandler(
    IDowntimeReasonCodeRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteDowntimeReasonCodeCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteDowntimeReasonCodeCommand cmd, CancellationToken ct)
    {
        var entity = await repo.GetByCodeAsync(cmd.Code, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound($"DowntimeReasonCode '{cmd.Code}' was not found.");

        entity.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
