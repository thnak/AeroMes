using AeroMes.Application.Common;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ProductionProcesses.Commands.DeleteProductionProcess;

public class DeleteProductionProcessHandler(IProductionProcessRepository repository)
    : ICommandHandler<DeleteProductionProcessCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteProductionProcessCommand command, CancellationToken ct)
    {
        var process = await repository.GetByIdAsync(command.ProcessID, ct);
        if (process is null) return ValidationResult<Unit>.NotFound($"Quy trình #{command.ProcessID} không tồn tại.");

        if (await repository.IsReferencedByWorkOrderAsync(command.ProcessID, ct))
            return ValidationResult<Unit>.Failure("Quy trình đang được sử dụng bởi lệnh sản xuất. Vui lòng ngừng hoạt động trước khi xóa.");

        await repository.DeleteAsync(process, ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
