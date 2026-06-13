using AeroMes.Application.Common;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.Criteria.Commands.DeleteCriteria;

public class DeleteCriteriaHandler(IQualityCriteriaRepository repository)
    : ICommandHandler<DeleteCriteriaCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteCriteriaCommand command, CancellationToken ct)
    {
        var criteria = await repository.GetByIdAsync(command.CriteriaID, ct);
        if (criteria is null) return ValidationResult<Unit>.NotFound($"Tiêu chí #{command.CriteriaID} không tồn tại.");

        if (await repository.HasInspectionReferencesAsync(command.CriteriaID, ct))
            return ValidationResult<Unit>.Failure("Tiêu chí đang được sử dụng trong dữ liệu kiểm tra. Vui lòng ngừng sử dụng thay vì xóa.");

        await repository.DeleteAsync(criteria, ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
