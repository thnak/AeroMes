using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.Queries.GetCutOrder;

public record GetCutOrderQuery(int CutOrderID) : IQuery<CutOrderDto?>;
