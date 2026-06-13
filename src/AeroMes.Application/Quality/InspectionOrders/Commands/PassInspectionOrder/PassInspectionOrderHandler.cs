using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Quality.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionOrders.Commands.PassInspectionOrder;

public class PassInspectionOrderHandler(
    IInspectionOrderRepository repo,
    IUnitOfWork uow,
    IValidator<PassInspectionOrderCommand> validator)
    : ICommandHandler<PassInspectionOrderCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(PassInspectionOrderCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        try
        {
            var order = await repo.GetByIdAsync(cmd.InspectionOrderId, ct);
            if (order is null)
                return ValidationResult<Unit>.NotFound($"InspectionOrder '{cmd.InspectionOrderId}' was not found.");

            order.Pass();
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
