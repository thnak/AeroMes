using AeroMes.Domain.Energy.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Energy.Queries.GetShiftEnergyReport;

public record GetShiftEnergyReportQuery(string? MachineCode, DateTime From, DateTime To)
    : IQuery<IReadOnlyList<ShiftEnergyDto>>;
