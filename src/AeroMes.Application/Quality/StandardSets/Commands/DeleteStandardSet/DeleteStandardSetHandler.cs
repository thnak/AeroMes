using AeroMes.Application.Common;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.StandardSets.Commands.DeleteStandardSet;

public class DeleteStandardSetHandler(IQualityStandardSetRepository repository)
    : ICommandHandler<DeleteStandardSetCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteStandardSetCommand command, CancellationToken ct)
    {
        var entity = await repository.GetByIdAsync(command.StandardSetID, ct);
        if (entity is null) return ValidationResult<Unit>.NotFound("Bộ tiêu chuẩn không tồn tại.");

        if (await repository.HasInspectionReferencesAsync(command.StandardSetID, ct))
            return ValidationResult<Unit>.Failure("Bộ tiêu chuẩn đang được tham chiếu bởi dữ liệu kiểm tra. Không thể xóa.");

        await repository.DeleteAsync(entity, ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
