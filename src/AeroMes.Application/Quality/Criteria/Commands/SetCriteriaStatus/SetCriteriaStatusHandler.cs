using AeroMes.Application.Common;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.Criteria.Commands.SetCriteriaStatus;

public class SetCriteriaStatusHandler(IQualityCriteriaRepository repository)
    : ICommandHandler<SetCriteriaStatusCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        SetCriteriaStatusCommand command, CancellationToken ct)
    {
        var criteria = await repository.GetByIdAsync(command.CriteriaID, ct);
        if (criteria is null) return ValidationResult<Unit>.NotFound($"Tiêu chí #{command.CriteriaID} không tồn tại.");

        if (command.Status == CriteriaStatus.Active) criteria.Activate(command.UpdatedBy);
        else criteria.Discontinue(command.UpdatedBy);

        await repository.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
