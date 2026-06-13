using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.Molds.Commands.CompleteMoldMaintenance;

public class CompleteMoldMaintenanceHandler(
    IMoldRepository repo,
    IUnitOfWork uow,
    IValidator<CompleteMoldMaintenanceCommand> validator) : ICommandHandler<CompleteMoldMaintenanceCommand, ValidationResult<long>>
{
    public async Task<ValidationResult<long>> HandleAsync(CompleteMoldMaintenanceCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<long>.Invalid(validation.ToErrorDictionary());

        try
        {
            var mold = await repo.GetByCodeAsync(cmd.MoldCode, ct);
            if (mold is null) return ValidationResult<long>.NotFound($"Entity '{cmd.MoldCode}' was not found.");

            var log = mold.CompleteMaintenance(
                cmd.MaintenanceType, cmd.StartDate, cmd.EndDate,
                cmd.TechnicianId, cmd.Description, cmd.PartReplaced,
                cmd.Cost, cmd.NextDueShots, cmd.UpdatedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<long>.Ok(log.LogId);
        }        catch (DomainException ex)
        {
            return ValidationResult<long>.Failure(ex.Message);
        }
    }
}
