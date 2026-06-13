using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Traceability.Commands.PlaceHold;

public sealed class PlaceHoldHandler(
    ILotHoldRepository repository,
    IValidator<PlaceHoldCommand> validator)
    : ICommandHandler<PlaceHoldCommand, ValidationResult<Guid>>
{
    public async Task<ValidationResult<Guid>> HandleAsync(
        PlaceHoldCommand command, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(command, ct);
        if (!vr.IsValid) return ValidationResult<Guid>.Invalid(vr.ToErrorDictionary());

        try
        {
            var hold = LotHold.Place(
                command.LotNumber, command.HoldReason, command.InitiatedBy,
                command.ProductCode, command.WorkOrderID,
                command.HoldDescription, command.HoldReference);

            await repository.AddAsync(hold, ct);
            await repository.SaveChangesAsync(ct);
            return ValidationResult<Guid>.Ok(hold.HoldID);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Guid>.Failure(ex.Message);
        }
    }
}
