using AeroMes.Application.Interfaces;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Events;
using AeroMes.Domain.Quality.Repositories;
using LiteBus.Events.Abstractions;
using Microsoft.Extensions.Logging;

namespace AeroMes.Application.Quality.Ncr.Events;

public class CreateNcrOnInspectionFailed(
    IInspectionResultRepository resultRepo,
    INcrRepository ncrRepo,
    IUnitOfWork uow,
    ILogger<CreateNcrOnInspectionFailed> logger)
    : IEventHandler<InspectionOrderFailedEvent>
{
    public async Task HandleAsync(InspectionOrderFailedEvent @event, CancellationToken ct)
    {
        // Guard: only auto-create if not already exists
        if (await ncrRepo.ExistsByOrderAsync(@event.InspectionOrderId, ct))
        {
            logger.LogInformation(
                "CreateNcrOnInspectionFailed: NCR already exists for InspectionOrder {OrderId}. Skipping.",
                @event.InspectionOrderId);
            return;
        }

        var results = await resultRepo.GetByOrderAsync(@event.InspectionOrderId, ct);

        // Count results where IsWithinSpec is explicitly false
        var failedCount = results.Count(r => r.IsWithinSpec == false);

        var ncrNo = $"NCR-{DateTime.UtcNow:yyyy}-{@event.InspectionOrderId:D5}";

        var ncr = Domain.Quality.Ncr.Create(
            ncrNo,
            @event.InspectionOrderId,
            @event.WorkOrderId,
            @event.ProductCode,
            null,
            failedCount > 0 ? failedCount : 1,
            "MAJOR",
            "system");

        ncrRepo.Add(ncr);
        await uow.SaveChangesAsync(ct);

        // Add defect lines — requires a second save so NcrId is set
        var failedWithDefect = results.Where(r => r.IsWithinSpec == false && r.DefectCodeId.HasValue).ToList();
        if (failedWithDefect.Count > 0)
        {
            foreach (var r in failedWithDefect)
            {
                var line = NcrDefectLine.Create(ncr.NcrId, r.DefectCodeId!.Value, 1, null);
                ncr.AddDefectLine(line);
            }
            await uow.SaveChangesAsync(ct);
        }

        logger.LogInformation(
            "CreateNcrOnInspectionFailed: Created NCR {NcrNo} for InspectionOrder {OrderId}.",
            ncrNo, @event.InspectionOrderId);
    }
}
