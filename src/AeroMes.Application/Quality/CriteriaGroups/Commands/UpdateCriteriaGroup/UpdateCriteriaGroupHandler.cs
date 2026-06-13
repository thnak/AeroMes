using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.CriteriaGroups.Commands.UpdateCriteriaGroup;

public class UpdateCriteriaGroupHandler(IQualityCriteriaGroupRepository repository)
    : ICommandHandler<UpdateCriteriaGroupCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateCriteriaGroupCommand command, CancellationToken ct)
    {
        var group = await repository.GetByIdAsync(command.GroupID, ct);
        if (group is null) return ValidationResult<Unit>.NotFound($"Nhóm tiêu chí #{command.GroupID} không tồn tại.");

        try
        {
            group.UpdateName(command.Name, command.UpdatedBy);
            await repository.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
