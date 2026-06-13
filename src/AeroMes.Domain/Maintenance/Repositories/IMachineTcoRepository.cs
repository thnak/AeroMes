namespace AeroMes.Domain.Maintenance.Repositories;

public record MachineTcoDto(
    string MachineCode, short PeriodYear, byte PeriodMonth,
    decimal PlannedMaintCost, decimal ActualMaintCost,
    int BreakdownCount, decimal? MtbfHours, decimal? MttrHours,
    DateTime LastRefreshedAt);

public interface IMachineTcoRepository
{
    Task<MachineTcoSummary?> GetByPeriodAsync(string machineCode, short year, byte month, CancellationToken ct);
    Task AddAsync(MachineTcoSummary summary, CancellationToken ct);
    Task<IReadOnlyList<MachineTcoDto>> GetTcoHistoryAsync(string machineCode, int months, CancellationToken ct);
}
