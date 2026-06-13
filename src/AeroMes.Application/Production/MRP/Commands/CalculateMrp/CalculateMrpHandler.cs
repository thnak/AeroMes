using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.MRP.Commands.CalculateMrp;

public class CalculateMrpHandler(
    IMaterialRequirementsPlanRepository repository,
    IInventoryStockRepository inventoryRepository)
    : ICommandHandler<CalculateMrpCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        CalculateMrpCommand command, CancellationToken ct)
    {
        var plan = await repository.GetByIdAsync(command.MrpID, ct);
        if (plan is null) return ValidationResult<Unit>.NotFound("Kế hoạch NVL không tồn tại.");
        if (!plan.MasterPlanId.HasValue)
            return ValidationResult<Unit>.Failure("Kế hoạch NVL phải có kế hoạch sản xuất chủ đạo.");

        try
        {
            var bomExplosion = await repository.ExplodeBomAsync(
                plan.MasterPlanId.Value, plan.PeriodStart, ct);

            var allStock = await inventoryRepository.GetAllNonZeroAsync(ct);
            var stockByCode = allStock
                .GroupBy(s => s.ProductCode)
                .ToDictionary(g => g.Key, g => g.Sum(s => s.Quantity));

            var lines = bomExplosion.Select(b =>
            {
                var wasteRatio = 1m + b.ScrapFactor / 100m;
                var openingInv = stockByCode.TryGetValue(b.MaterialCode, out var stock) ? stock : 0m;
                return MrpLine.Create(
                    plan.MrpID, b.FinishedGoodCode, b.PlannedQty,
                    b.MaterialCode, b.MaterialName, b.UoM,
                    0m, wasteRatio * b.RequiredQtyPerUnit,
                    openingInv, 0m);
            }).ToList();

            plan.SetLines(lines, command.UpdatedBy);
            plan.MarkCalculated(command.UpdatedBy);
            await repository.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
