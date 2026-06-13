using AeroMes.Domain.Master.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Master.ProductionProcesses.Queries.GetProductionProcessDetail;

public record GetProductionProcessDetailQuery(int ProcessID) : IQuery<ProductionProcessDetailDto?>;
