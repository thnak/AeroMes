using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.InspectionResults.Queries.GetInspectionResults;

public record GetInspectionResultsQuery(int InspectionOrderId)
    : IQuery<IReadOnlyList<InspectionResultDto>>;
