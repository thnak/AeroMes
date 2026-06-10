using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Routings.Queries.GetRoutings;

public record GetRoutingsQuery(bool ActiveOnly = true) : IQuery<IReadOnlyList<RoutingDto>>;

public record RoutingDto(
    int RoutingID,
    string RoutingCode,
    string RoutingName,
    string ProductCode,
    bool IsDefault,
    bool IsActive);
