namespace AeroMes.Application.Labeling.Services;

public sealed record LabelRenderResult(byte[] Data, string ContentType);

public interface ILabelRenderer
{
    LabelRenderResult RenderCompact(
        string qrPayload,
        IReadOnlyDictionary<string, string> fields,
        string outputFormat);

    LabelRenderResult RenderComposite(
        string qrPayload,
        IReadOnlyDictionary<string, string> fields,
        string outputFormat);
}
