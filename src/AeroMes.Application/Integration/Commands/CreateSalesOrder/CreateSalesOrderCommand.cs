using AeroMes.Application.Common;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Integration.Commands.CreateSalesOrder;

public record SalesOrderLineInput(
    string ProductCode,
    string? ProductName,
    decimal Quantity,
    string? Unit,
    decimal UnitPrice = 0,
    string? Notes = null);

public record CreateSalesOrderCommand(
    string? SoCode,
    string? CustomerCode,
    string? CustomerName,
    DateTime OrderDate,
    DateTime? DeliveryDate,
    string? Notes,
    IReadOnlyList<SalesOrderLineInput> Lines,
    string? CreatedBy) : ICommand<ValidationResult<int>>;

public class CreateSalesOrderValidator : AbstractValidator<CreateSalesOrderCommand>
{
    public CreateSalesOrderValidator()
    {
        RuleFor(x => x.OrderDate).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one product line is required.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductCode).NotEmpty().MaximumLength(100);
            line.RuleFor(l => l.Quantity).GreaterThan(0);
        });
    }
}
