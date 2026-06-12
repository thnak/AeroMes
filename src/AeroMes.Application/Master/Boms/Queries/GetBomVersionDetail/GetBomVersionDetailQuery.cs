using AeroMes.Application.Master.Boms.Queries.GetActiveBom;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.Boms.Queries.GetBomVersionDetail;

public record GetBomVersionDetailQuery(string ProductCode, string Version) : IQuery<BomVersionDetailDto?>;
