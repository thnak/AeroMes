using AeroMes.Application.Interfaces;
using AeroMes.Application.Wms.Services;
using AeroMes.Domain.Production.Repositories;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;

namespace AeroMes.Infrastructure.Services;

public class StockPolicyEvaluationService(
    IStockPolicyRepository policyRepo,
    IReplenishmentAlertRepository alertRepo,
    IInventoryStockRepository stockRepo,
    IUnitOfWork uow) : IStockPolicyEvaluationService
{
    public async Task EvaluateAsync(string productCode, int locationId, CancellationToken ct)
    {
        var policy = await policyRepo.GetActiveByProductAndLocationAsync(productCode, locationId, ct);
        if (policy is null) return;

        var currentQty = await stockRepo.GetTotalQtyAsync(locationId, productCode, ct);
        var openAlert = await alertRepo.GetOpenByPolicyAsync(policy.PolicyId, ct);

        if (currentQty <= policy.MinQty && openAlert is null)
        {
            var alert = ReplenishmentAlert.Create(policy.PolicyId, productCode, locationId, currentQty);
            await alertRepo.AddAsync(alert, ct);
            await uow.SaveChangesAsync(ct);
        }
        else if (currentQty > policy.MinQty && openAlert is not null)
        {
            openAlert.Resolve();
            await uow.SaveChangesAsync(ct);
        }
    }
}
