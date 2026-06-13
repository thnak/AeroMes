using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetRacks;

public record GetRacksQuery(int AisleId) : IQuery<IReadOnlyList<RackDto>>;

public record RackDto(int RackId, int AisleId, string RackCode, decimal? MaxWeightKg);
