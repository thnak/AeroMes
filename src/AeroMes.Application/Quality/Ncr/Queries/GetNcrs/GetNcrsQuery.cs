using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Quality.Ncr.Queries.GetNcrs;

public record GetNcrsQuery(string? Status, string? ProductCode) : IQuery<IReadOnlyList<NcrListDto>>;
