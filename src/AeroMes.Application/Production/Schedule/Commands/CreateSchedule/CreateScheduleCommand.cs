using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Schedule.Commands.CreateSchedule;

public record CreateScheduleCommand(
    string? ScheduleName, string? FacilityCode,
    DateTime PeriodStart, DateTime PeriodEnd,
    string? CreatedBy) : ICommand<ValidationResult<int>>;

public class CreateScheduleCommandHandler(IProductionScheduleRepository repo)
    : ICommandHandler<CreateScheduleCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        CreateScheduleCommand cmd, CancellationToken ct = default)
    {
        var name = string.IsNullOrWhiteSpace(cmd.ScheduleName)
            ? $"Schedule {cmd.PeriodStart:yyyy-MM-dd} – {cmd.PeriodEnd:yyyy-MM-dd}"
            : cmd.ScheduleName;
        try
        {
            var schedule = ProductionSchedule.Create(name, cmd.FacilityCode, cmd.PeriodStart, cmd.PeriodEnd, cmd.CreatedBy);
            var id = await repo.AddAsync(schedule, ct);
            return ValidationResult<int>.Ok(id);
        }
        catch (Exception ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
