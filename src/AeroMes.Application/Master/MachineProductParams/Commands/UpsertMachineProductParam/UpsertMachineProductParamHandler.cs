using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.MachineProductParams.Commands.UpsertMachineProductParam;

public class UpsertMachineProductParamHandler(
    IMachineProductParamRepository repo,
    IUnitOfWork uow,
    IValidator<UpsertMachineProductParamCommand> validator) : ICommandHandler<UpsertMachineProductParamCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(UpsertMachineProductParamCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var existing = await repo.GetAsync(cmd.MachineCode, cmd.ProductCode, cmd.ParamName, ct);
            if (existing is not null)
            {
                existing.Update(cmd.Unit, cmd.NominalValue, cmd.MinValue, cmd.MaxValue, cmd.IsControlParam, cmd.DisplayOrder);
            }
            else
            {
                var entity = MachineProductParam.Create(
                    cmd.MachineCode, cmd.ProductCode, cmd.ParamName,
                    cmd.Unit, cmd.NominalValue, cmd.MinValue, cmd.MaxValue,
                    cmd.IsControlParam, cmd.DisplayOrder);
                await repo.AddAsync(entity, ct);
            }
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
