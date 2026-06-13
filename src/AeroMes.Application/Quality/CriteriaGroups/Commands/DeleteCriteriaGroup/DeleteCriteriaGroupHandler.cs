using AeroMes.Application.Common;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.CriteriaGroups.Commands.DeleteCriteriaGroup;

public class DeleteCriteriaGroupHandler(IQualityCriteriaGroupRepository repository)
    : ICommandHandler<DeleteCriteriaGroupCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteCriteriaGroupCommand command, CancellationToken ct)
    {
        var group = await repository.GetByIdAsync(command.GroupID, ct);
        if (group is null) return ValidationResult<Unit>.NotFound($"Nhóm tiêu chí #{command.GroupID} không tồn tại.");

        if (await repository.HasCriteriaReferencesAsync(command.GroupID, ct))
            return ValidationResult<Unit>.Failure("Không thể xóa nhóm đang được sử dụng bởi tiêu chí chất lượng.");

        await repository.DeleteAsync(group, ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
