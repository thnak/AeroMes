namespace AeroMes.Application.Iot.Signals.Queries.GetSignals;

public record SignalDto(
    int SignalId,
    int AdapterId,
    string TagKey,
    string DisplayName,
    string SourceAddress,
    double Scale,
    double Offset,
    double? QualityMin,
    double? QualityMax,
    bool IsEnabled);
