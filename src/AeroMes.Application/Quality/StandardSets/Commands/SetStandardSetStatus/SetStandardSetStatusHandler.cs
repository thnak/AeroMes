using AeroMes.Application.Common;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.StandardSets.Commands.SetStandardSetStatus;

public class SetStandardSetStatusHandler(IQualityStandardSetRepository repository)
    : ICommandHandler<SetStandardSetStatusCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        SetStandardSetStatusCommand command, CancellationToken ct)
    {
        var entity = await repository.GetByIdAsync(command.StandardSetID, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound("Bộ tiêu chuẩn không tồn tại.");

        if (command.Status == StandardSetStatus.Active)
            entity.Activate(command.UpdatedBy);
        else
            entity.Discontinue(command.UpdatedBy);

        await repository.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
