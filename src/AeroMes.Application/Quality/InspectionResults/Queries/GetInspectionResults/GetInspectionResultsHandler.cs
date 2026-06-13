using AeroMes.Domain.Quality.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.InspectionResults.Queries.GetInspectionResults;

public class GetInspectionResultsHandler(
    IInspectionOrderRepository orderRepo,
    IInspectionPlanRepository planRepo,
    IInspectionResultRepository resultRepo)
    : IQueryHandler<GetInspectionResultsQuery, IReadOnlyList<InspectionResultDto>>
{
    public async Task<IReadOnlyList<InspectionResultDto>> HandleAsync(
        GetInspectionResultsQuery query, CancellationToken ct)
    {
        var order = await orderRepo.GetByIdAsync(query.InspectionOrderId, ct);
        if (order is null) return [];

        var results = await resultRepo.GetByOrderAsync(query.InspectionOrderId, ct);
        if (results.Count == 0) return [];

        var plan = await planRepo.GetByIdWithCharacteristicsAsync(order.PlanId, ct);
        var charMap = plan?.Characteristics.ToDictionary(c => c.CharId) ?? [];

        return results.Select(r =>
        {
            charMap.TryGetValue(r.CharId, out var ch);
            return new InspectionResultDto(
                r.ResultId,
                r.InspectionOrderId,
                r.CharId,
                ch?.CharName ?? r.CharId.ToString(),
                ch?.MeasurementType ?? "",
                r.MeasuredValue,
                r.AttributeResult,
                r.IsWithinSpec,
                null,
                r.SampleIndex,
                r.Notes,
                r.RecordedBy,
                r.RecordedAt);
        }).ToList();
    }
}
