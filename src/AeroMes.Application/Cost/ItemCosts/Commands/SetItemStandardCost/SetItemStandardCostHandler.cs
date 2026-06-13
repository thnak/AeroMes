using AeroMes.Application.Common;
using AeroMes.Domain.Cost;
using AeroMes.Domain.Cost.Repositories;
using AeroMes.Domain.Exceptions;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Cost.ItemCosts.Commands.SetItemStandardCost;

public class SetItemStandardCostHandler(
    IItemCostHistoryRepository repository,
    IValidator<SetItemStandardCostCommand> validator)
    : ICommandHandler<SetItemStandardCostCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        SetItemStandardCostCommand command, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(command, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        try
        {
            var existing = await repository.GetActiveAsync(command.ProductCode, command.CostType, ct);
            if (existing is not null)
            {
                if (existing.EffectiveFrom >= command.EffectiveFrom)
                    return ValidationResult<int>.Failure("Ngày hiệu lực mới phải sau ngày hiệu lực hiện tại.");
                existing.ExpireOn(command.EffectiveFrom.AddDays(-1));
            }

            var cost = ItemCostHistory.Create(
                command.ProductCode, command.CostType, command.UnitCost,
                command.CostUoM, command.EffectiveFrom,
                command.SourceRef, command.ApprovedBy, command.CreatedBy);

            var id = await repository.AddAsync(cost, ct);
            return ValidationResult<int>.Ok(id);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
