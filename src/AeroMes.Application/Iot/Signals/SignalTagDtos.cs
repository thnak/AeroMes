namespace AeroMes.Application.Iot.Signals;

public record SignalTagDto(int TagId, string Key, string DisplayName, string Category, string DataType,
    string? DefaultUnit, decimal? TypicalMin, decimal? TypicalMax, string? Description, bool IsSystemDefined);
