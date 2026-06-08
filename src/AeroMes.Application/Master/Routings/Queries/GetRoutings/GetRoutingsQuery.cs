using MediatR;

namespace AeroMes.Application.Master.Routings.Queries.GetRoutings;

public record GetRoutingsQuery(bool ActiveOnly = true) : IRequest<IReadOnlyList<RoutingDto>>;

public record RoutingDto(
    int RoutingID,
    string RoutingCode,
    string RoutingName,
    string ProductCode,
    bool IsDefault,
    bool IsActive);
