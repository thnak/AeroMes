using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Tools.Commands.DeleteTool;

public class DeleteToolHandler(
    IToolRepository repo,
    IUnitOfWork uow) : ICommandHandler<DeleteToolCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(DeleteToolCommand cmd, CancellationToken ct)
    {
        var tool = await repo.GetByCodeAsync(cmd.ToolCode, ct);
        if (tool is null) return ValidationResult<Unit>.NotFound($"Entity '{cmd.ToolCode}' was not found.");

        if (tool.Status == ToolStatus.InUse)
            throw new DomainException(
                $"Phải trả dụng cụ '{tool.ToolCode}' về kho trước khi xóa.");

        tool.SoftDelete(cmd.DeletedBy);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
