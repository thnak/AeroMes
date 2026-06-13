using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Integration;
using AeroMes.Domain.Integration.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.CreateSalesOrder;

public class CreateSalesOrderHandler(
    ISalesOrderRepository repo,
    IUnitOfWork uow,
    IValidator<CreateSalesOrderCommand> validator) : ICommandHandler<CreateSalesOrderCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        CreateSalesOrderCommand cmd, CancellationToken ct = default)
    {
        var v = await validator.ValidateAsync(cmd, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        var soCode = string.IsNullOrWhiteSpace(cmd.SoCode)
            ? await repo.NextSoCodeAsync(ct)
            : cmd.SoCode.Trim().ToUpperInvariant();

        if (await repo.GetByCodeAsync(soCode, ct) is not null)
            return ValidationResult<int>.Failure($"Sales order code '{soCode}' already exists.");

        var so = SalesOrder.CreateManual(
            soCode, cmd.CustomerCode, cmd.CustomerName,
            cmd.OrderDate, cmd.DeliveryDate, cmd.Notes, cmd.CreatedBy);

        await repo.AddAsync(so, ct);
        await uow.SaveChangesAsync(ct);

        foreach (var li in cmd.Lines)
        {
            var line = SalesOrderLine.Create(
                so.SOID, li.ProductCode, li.ProductName,
                li.Quantity, li.Unit, li.UnitPrice, li.Notes);
            so.AddLine(line);
        }

        await uow.SaveChangesAsync(ct);
        return ValidationResult<int>.Ok(so.SOID);
    }
}
