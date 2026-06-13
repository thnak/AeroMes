using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetCutOrdersByWO;

public record GetCutOrdersByWOQuery(int WOID) : IQuery<IReadOnlyList<CutOrderDto>>;
