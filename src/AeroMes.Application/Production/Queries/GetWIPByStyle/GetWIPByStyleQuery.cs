using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetWIPByStyle;

public record GetWIPByStyleQuery(
    string StyleCode,
    string? ColorCode = null,
    int? WOID = null) : IQuery<IReadOnlyList<WIPByStyleDto>>;
