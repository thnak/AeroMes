using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Traceability;
using AeroMes.Domain.Traceability.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using LiteBus.Events.Abstractions;
using AeroMes.Domain.Traceability.Events;

namespace AeroMes.Application.Traceability.Commands.IdentifyRecallScope;

public sealed class IdentifyRecallScopeHandler(
    IRecallRepository recallRepository,
    ILotTraceabilityRepository traceRepository,
    IProcessRecordRepository processRecordRepository,
    IValidator<IdentifyRecallScopeCommand> validator)
    : ICommandHandler<IdentifyRecallScopeCommand, ValidationResult<RecallScopeDto>>
{
    public async Task<ValidationResult<RecallScopeDto>> HandleAsync(
        IdentifyRecallScopeCommand command, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(command, ct);
        if (!vr.IsValid) return ValidationResult<RecallScopeDto>.Invalid(vr.ToErrorDictionary());

        var recall = await recallRepository.GetByIdAsync(command.RecallID, ct);
        if (recall is null) return ValidationResult<RecallScopeDto>.NotFound($"Recall {command.RecallID} not found.");

        try
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Run trace based on anchor direction
            var genealogy = recall.AnchorDirection switch
            {
                AnchorDirection.Backward => await traceRepository.BackwardTraceAsync(recall.AnchorLotNumber, 50, ct),
                AnchorDirection.Forward => await traceRepository.ForwardTraceAsync(recall.AnchorLotNumber, 50, ct),
                _ => await traceRepository.BidirectionalTraceAsync(recall.AnchorLotNumber, 50, ct),
            };

            // Get mid-session WIP lots
            var wipRecords = await processRecordRepository.GetMidSessionWIPAsync(null, null, ct);
            var wipLots = wipRecords.Select(r => r.LotNumber).ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Build scope lots
            var scopeLots = new List<RecallScopeLot>();

            foreach (var node in genealogy.Nodes)
            {
                var lotNumber = node.LotNumber;
                var category = ClassifyLot(lotNumber, recall.AnchorLotNumber, wipLots, node.Depth);

                scopeLots.Add(RecallScopeLot.Create(
                    recall.RecallID, lotNumber, category, node.Depth, node.ProductCode));
            }

            sw.Stop();

            // Persist scope lots
            foreach (var sl in scopeLots)
                await recallRepository.AddScopeLotAsync(sl, ct);

            var wipCount = scopeLots.Count(l => l.LotCategory == LotCategory.WIPInProcess);
            var shippedCount = scopeLots.Count(l => l.LotCategory == LotCategory.Shipped);

            recall.MarkScopeIdentified(scopeLots.Count, wipCount, shippedCount);

            await recallRepository.AddAuditEntryAsync(
                RecallAuditEntry.Log(recall.RecallID, "ScopeIdentified", command.RequestedBy,
                    $"Identified {scopeLots.Count} affected lots ({wipCount} WIP, {shippedCount} shipped) in {sw.ElapsedMilliseconds}ms",
                    systemGenerated: true),
                ct);

            await recallRepository.SaveChangesAsync(ct);

            // Check for shipped lots and emit event
            var customerRefs = scopeLots
                .Where(l => l.LotCategory == LotCategory.Shipped && l.CustomerRef is not null)
                .Select(l => l.CustomerRef!)
                .Distinct()
                .ToList();

            var scopeDto = new RecallScopeDto(
                recall.RecallID,
                recall.AnchorLotNumber,
                scopeLots.Count,
                wipCount,
                scopeLots.Count(l => l.LotCategory is LotCategory.FinishedGoods or LotCategory.RawMaterial),
                shippedCount,
                [.. scopeLots.Select(sl => new RecallScopeLotDto(
                    sl.RecallScopeLotID, sl.RecallID, sl.LotNumber, sl.ProductCode,
                    sl.TraceDepth, sl.LotCategory.ToString(), sl.CurrentLocationCode,
                    sl.QtyOnHand, sl.ShipmentRef, sl.CustomerRef, sl.HoldID, sl.AddedAt))],
                sw.ElapsedMilliseconds);

            return ValidationResult<RecallScopeDto>.Ok(scopeDto);
        }
        catch (DomainException ex)
        {
            return ValidationResult<RecallScopeDto>.Failure(ex.Message);
        }
    }

    private static LotCategory ClassifyLot(
        string lotNumber,
        string anchorLot,
        HashSet<string> wipLots,
        int depth)
    {
        if (string.Equals(lotNumber, anchorLot, StringComparison.OrdinalIgnoreCase))
            return LotCategory.AnchorLot;
        if (wipLots.Contains(lotNumber))
            return LotCategory.WIPInProcess;
        if (depth == 0)
            return LotCategory.FinishedGoods;
        return LotCategory.Unknown;
    }
}
