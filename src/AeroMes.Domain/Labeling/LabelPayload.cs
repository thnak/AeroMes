namespace AeroMes.Domain.Labeling;

public sealed record LabelPayload(
    string Version,
    string ContentType,
    string[] Fields)
{
    public string Encode() => $"{Version}_{ContentType}_{string.Join("|", Fields)}";
}
