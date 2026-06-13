using AeroMes.Domain.Iot;

namespace AeroMes.Application.Iot.Adapters.Queries.GetAdapters;

public record AdapterDto(
    int AdapterId,
    string MachineCode,
    AdapterType AdapterType,
    AdapterStatus Status,
    bool IsEnabled,
    DateTime? LastSignalAt,
    int SignalCount,
    string? WebhookApiKey);
