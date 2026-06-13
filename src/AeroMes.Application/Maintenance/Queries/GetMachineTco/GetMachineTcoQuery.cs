using AeroMes.Domain.Maintenance.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Maintenance.Queries.GetMachineTco;

public record GetMachineTcoQuery(string MachineCode, int Months = 12)
    : IQuery<IReadOnlyList<MachineTcoDto>>;
