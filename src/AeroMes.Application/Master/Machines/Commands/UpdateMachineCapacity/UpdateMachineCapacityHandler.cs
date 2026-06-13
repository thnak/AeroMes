using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.Machines.Commands.UpdateMachineCapacity;

public class UpdateMachineCapacityHandler(
    IMachineRepository repo,
    IUnitOfWork uow,
    IValidator<UpdateMachineCapacityCommand> validator) : ICommandHandler<UpdateMachineCapacityCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpdateMachineCapacityCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var entity = await repo.GetByCodeAsync(cmd.Code, ct)
                ?? throw new EntityNotFoundException("Machine", cmd.Code);

            entity.UpdateCapacity(
                cmd.MachineCategory,
                cmd.TargetOeePct,
                cmd.TheoreticalCapacityPerHour,
                cmd.PlannedDowntimeMinPerShift,
                cmd.HourlyCostRate,
                cmd.OpcUaNodeId,
                cmd.RequiresCertification,
                cmd.CertificationCode,
                cmd.MaxOperators,
                cmd.UpdatedBy);

            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (EntityNotFoundException ex)
        {
            return ValidationResult<Unit>.NotFound(ex.Message);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
