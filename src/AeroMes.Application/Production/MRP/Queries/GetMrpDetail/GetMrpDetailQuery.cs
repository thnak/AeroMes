using AeroMes.Domain.Production.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Production.MRP.Queries.GetMrpDetail;

public record GetMrpDetailQuery(int MrpID) : IQuery<MrpDetailDto?>;
