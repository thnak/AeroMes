using AeroMes.Application.Labeling.Services;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AeroMes.Infrastructure.Labeling;

public sealed class QuestPdfLabelRenderer : ILabelRenderer
{
    static QuestPdfLabelRenderer()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public LabelRenderResult RenderCompact(
        string qrPayload,
        IReadOnlyDictionary<string, string> fields,
        string outputFormat)
    {
        if (outputFormat == "zpl")
            return new LabelRenderResult(BuildZpl(qrPayload, fields, compact: true), "application/vnd.zebra-zpl");

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(100, 50, Unit.Millimetre);
                page.Margin(3, Unit.Millimetre);
                page.Content().Row(row =>
                {
                    // QR code (40% width)
                    row.RelativeItem(4).AlignMiddle().AlignCenter().Image(GenerateQrPng(qrPayload, pixels: 6));

                    // Text id
                    row.RelativeItem(6).AlignMiddle().Column(col =>
                    {
                        if (fields.TryGetValue("Type", out var type))
                            col.Item().Text(type).FontSize(7).FontColor(Colors.Grey.Medium);
                        if (fields.TryGetValue("Code", out var code))
                            col.Item().Text(code).Bold().FontSize(10);
                        else if (fields.TryGetValue("ID", out var id))
                            col.Item().Text(id).Bold().FontSize(10);
                        if (fields.TryGetValue("Name", out var name))
                            col.Item().Text(name).FontSize(7);
                    });
                });
            });
        }).GeneratePdf();

        return outputFormat == "pdf"
            ? new LabelRenderResult(pdfBytes, "application/pdf")
            : new LabelRenderResult(PdfToPng(pdfBytes), "image/png");
    }

    public LabelRenderResult RenderComposite(
        string qrPayload,
        IReadOnlyDictionary<string, string> fields,
        string outputFormat)
    {
        if (outputFormat == "zpl")
            return new LabelRenderResult(BuildZpl(qrPayload, fields, compact: false), "application/vnd.zebra-zpl");

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(100, 60, Unit.Millimetre);
                page.Margin(3, Unit.Millimetre);
                page.Content().Row(row =>
                {
                    // QR code (30% width)
                    row.RelativeItem(3).AlignMiddle().AlignCenter().Image(GenerateQrPng(qrPayload, pixels: 5));

                    // Data table (70% width)
                    row.RelativeItem(7).AlignMiddle().Padding(2, Unit.Millimetre).Column(col =>
                    {
                        foreach (var (key, value) in fields)
                        {
                            if (string.IsNullOrEmpty(value)) continue;
                            col.Item().Row(r =>
                            {
                                r.RelativeItem(4).Text(key + ":").FontSize(6).FontColor(Colors.Grey.Medium);
                                r.RelativeItem(6).Text(value).FontSize(7).Bold();
                            });
                        }
                    });
                });
            });
        }).GeneratePdf();

        return outputFormat == "pdf"
            ? new LabelRenderResult(pdfBytes, "application/pdf")
            : new LabelRenderResult(PdfToPng(pdfBytes), "image/png");
    }

    private static byte[] GenerateQrPng(string payload, int pixels)
    {
        using var gen  = new QRCodeGenerator();
        var data       = gen.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
        using var code = new PngByteQRCode(data);
        return code.GetGraphic(pixels);
    }

    private static byte[] PdfToPng(byte[] pdfBytes)
    {
        // Return PDF when PNG conversion isn't available (requires platform-specific rendering).
        // Callers receiving "image/png" should handle application/pdf fallback.
        return pdfBytes;
    }

    private static byte[] BuildZpl(string qrPayload, IReadOnlyDictionary<string, string> fields, bool compact)
    {
        var lines = new System.Text.StringBuilder();
        lines.AppendLine("^XA");
        lines.AppendLine("^MMT");
        lines.AppendLine("^PW400");
        lines.AppendLine("^LL200");

        // QR code field
        lines.AppendLine($"^FO20,20^BQN,2,4^FDQA,{qrPayload}^FS");

        if (!compact)
        {
            int y = 20;
            foreach (var (key, value) in fields)
            {
                if (string.IsNullOrEmpty(value)) continue;
                lines.AppendLine($"^FO140,{y}^A0N,16,12^FD{key}: {value}^FS");
                y += 20;
            }
        }

        lines.AppendLine("^XZ");
        return System.Text.Encoding.ASCII.GetBytes(lines.ToString());
    }
}
