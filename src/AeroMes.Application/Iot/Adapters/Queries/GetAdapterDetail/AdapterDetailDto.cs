using AeroMes.Application.Iot.Signals.Queries.GetSignals;
using AeroMes.Domain.Iot;

namespace AeroMes.Application.Iot.Adapters.Queries.GetAdapterDetail;

public record AdapterDetailDto(
    int AdapterId,
    string MachineCode,
    AdapterType AdapterType,
    string ConfigJson,
    AdapterStatus Status,
    bool IsEnabled,
    DateTime? LastSignalAt,
    string? WebhookApiKey,
    IReadOnlyList<SignalDto> Signals);
