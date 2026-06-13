using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Wms.Queries.GetAisles;

public record GetAislesQuery(int ZoneId) : IQuery<IReadOnlyList<AisleDto>>;

public record AisleDto(int AisleId, int ZoneId, string AisleCode, int PickSequence);
