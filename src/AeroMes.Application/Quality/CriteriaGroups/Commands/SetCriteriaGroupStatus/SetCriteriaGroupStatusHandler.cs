using AeroMes.Application.Common;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.CriteriaGroups.Commands.SetCriteriaGroupStatus;

public class SetCriteriaGroupStatusHandler(IQualityCriteriaGroupRepository repository)
    : ICommandHandler<SetCriteriaGroupStatusCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        SetCriteriaGroupStatusCommand command, CancellationToken ct)
    {
        var group = await repository.GetByIdAsync(command.GroupID, ct);
        if (group is null) return ValidationResult<Unit>.NotFound($"Nhóm tiêu chí #{command.GroupID} không tồn tại.");

        group.SetStatus(command.Status, command.UpdatedBy);
        await repository.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
