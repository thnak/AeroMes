using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Domain.Exceptions;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.Commands.CreateReworkOrder;

public class CreateReworkOrderHandler(
    IReworkOrderRepository repository,
    IUnitOfWork uow,
    IValidator<CreateReworkOrderCommand> validator)
    : ICommandHandler<CreateReworkOrderCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateReworkOrderCommand command, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(command, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        if (await repository.CodeExistsAsync(command.ReworkCode, ct))
            return ValidationResult<int>.Failure($"Mã lệnh tái chế '{command.ReworkCode}' đã tồn tại.");

        try
        {
            var order = ReworkOrder.Create(
                command.ReworkCode, command.SourceWOID, command.ScrapTxID,
                command.ProductCode, command.ReworkQty, command.ReworkStepFromId,
                command.CreatedBy);

            await repository.AddAsync(order, ct);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(order.ReworkID);
        }
        catch (DomainException ex) { return ValidationResult<int>.Failure(ex.Message); }
    }
}
