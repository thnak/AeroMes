using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Tools.Commands.RecordToolMaintenance;

public class RecordToolMaintenanceHandler(
    IToolRepository repo,
    IUnitOfWork uow,
    IValidator<RecordToolMaintenanceCommand> validator) : ICommandHandler<RecordToolMaintenanceCommand, ValidationResult<long>>
{
    public async Task<ValidationResult<long>> HandleAsync(RecordToolMaintenanceCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<long>.Invalid(validation.ToErrorDictionary());

        try
        {
            var tool = await repo.GetByCodeAsync(cmd.ToolCode, ct);
            if (tool is null) return ValidationResult<long>.NotFound($"Entity '{cmd.ToolCode}' was not found.");

            var log = tool.RecordMaintenance(
                cmd.MaintenanceType, cmd.PerformedAt, cmd.PerformedBy,
                cmd.Cost, cmd.NextDueCount, cmd.NextDueDate, cmd.Notes, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<long>.Ok(log.LogId);
        }        catch (DomainException ex)
        {
            return ValidationResult<long>.Failure(ex.Message);
        }
    }
}
