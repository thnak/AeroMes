using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetPackagingBoms;

public record GetPackagingBomsQuery(string? ProductCode = null)
    : IQuery<IReadOnlyList<PackagingBomDto>>;
