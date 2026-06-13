using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.GenerateCycleCountLines;

public class GenerateCycleCountLinesHandler(
    ICycleCountPlanRepository planRepo,
    IInventoryStockRepository stockRepo,
    IUnitOfWork uow)
    : ICommandHandler<GenerateCycleCountLinesCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        GenerateCycleCountLinesCommand cmd, CancellationToken ct)
    {
        try
        {
            var plan = await planRepo.GetByIdWithLinesAsync(cmd.PlanId, ct);
            if (plan is null)
                return ValidationResult<Unit>.NotFound($"Không tìm thấy kế hoạch kiểm kê #{cmd.PlanId}.");

            var stocks = cmd.BinIds is { Length: > 0 }
                ? await stockRepo.GetByBinsAsync(cmd.BinIds, ct)
                : await stockRepo.GetAllNonZeroAsync(ct);

            if (stocks.Count == 0)
                return ValidationResult<Unit>.Failure("Không có tồn kho nào phù hợp để tạo dòng kiểm kê.");

            plan.ClearLines();
            foreach (var s in stocks.Where(s => s.BinId.HasValue))
                plan.AddLine(s.BinId!.Value, s.LocationID, s.ProductCode, s.LotNumber, s.Quantity);

            plan.StartCount();
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
