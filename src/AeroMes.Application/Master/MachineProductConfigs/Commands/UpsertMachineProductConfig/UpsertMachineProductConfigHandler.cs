using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Master;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using AeroMes.Domain.Exceptions;

namespace AeroMes.Application.Master.MachineProductConfigs.Commands.UpsertMachineProductConfig;
public class UpsertMachineProductConfigHandler(
    IMachineProductConfigRepository repo,
    IUnitOfWork uow,
    IValidator<UpsertMachineProductConfigCommand> validator) : ICommandHandler<UpsertMachineProductConfigCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpsertMachineProductConfigCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());
        try
        {
            var existing = await repo.GetAsync(cmd.MachineCode, cmd.ProductCode, ct);
            if (existing is not null)
            {
                existing.Update(cmd.IdealCycleTimeSeconds, cmd.TargetThroughputPerHour,
                    cmd.SetupTimeSeconds, cmd.EffectiveFrom);
            }
            else
            {
                var entity = MachineProductConfig.Create(
                    cmd.MachineCode, cmd.ProductCode,
                    cmd.IdealCycleTimeSeconds, cmd.TargetThroughputPerHour,
                    cmd.SetupTimeSeconds, cmd.EffectiveFrom, cmd.RoutingStepId);
                await repo.AddAsync(entity, ct);
            }
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
