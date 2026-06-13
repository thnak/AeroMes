using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Tools.Commands.RecordToolUsage;

public class RecordToolUsageHandler(
    IToolRepository repo,
    IUnitOfWork uow,
    IValidator<RecordToolUsageCommand> validator) : ICommandHandler<RecordToolUsageCommand, ValidationResult<RecordToolUsageResult>>
{
    public async Task<ValidationResult<RecordToolUsageResult>> HandleAsync(RecordToolUsageCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<RecordToolUsageResult>.Invalid(validation.ToErrorDictionary());

        try
        {
            var tool = await repo.GetByCodeAsync(cmd.ToolCode, ct);
            if (tool is null) return ValidationResult<RecordToolUsageResult>.NotFound($"Entity '{cmd.ToolCode}' was not found.");

            tool.AccumulateUsage(cmd.Count, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);

            var result = new RecordToolUsageResult(
                tool.CurrentUsageCount, tool.MaxUsageCount,
                tool.IsReconditioningDue, tool.IsNearingEndOfLife);
            return ValidationResult<RecordToolUsageResult>.Ok(result);
        }        catch (DomainException ex)
        {
            return ValidationResult<RecordToolUsageResult>.Failure(ex.Message);
        }
    }
}
