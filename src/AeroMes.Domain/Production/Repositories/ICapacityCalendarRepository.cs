namespace AeroMes.Domain.Production.Repositories;

public record CapacityCalendarDto(
    int WorkCenterID,
    string WorkCenterCode,
    string WorkCenterName,
    DateOnly CalendarDate,
    int ShiftTemplateId,
    int AvailableMinutes,
    bool IsWorkingDay,
    string? Notes);

public interface ICapacityCalendarRepository
{
    Task<CapacityCalendar?> GetAsync(int workCenterId, DateOnly date, int shiftTemplateId, CancellationToken ct);
    Task<IReadOnlyList<CapacityCalendar>> GetRangeAsync(int workCenterId, DateOnly from, DateOnly to, CancellationToken ct);
    Task AddAsync(CapacityCalendar entry, CancellationToken ct);
    Task<IReadOnlyList<CapacityCalendarDto>> GetListAsync(DateOnly? from, DateOnly? to, int? workCenterId, CancellationToken ct);
    Task<Dictionary<int, int>> GetAvailableMinutesOnDateAsync(DateOnly date, CancellationToken ct);
}
