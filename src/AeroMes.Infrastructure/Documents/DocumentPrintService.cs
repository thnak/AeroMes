using AeroMes.Application.Documents;
using AeroMes.Application.Storage;
using AeroMes.Domain.Templates;
using AeroMes.Infrastructure.Data;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.RegularExpressions;

namespace AeroMes.Infrastructure.Documents;

public sealed class DocumentPrintService(AppDbContext db, IFileStorage fileStorage)
    : IDocumentPrintService
{
    static DocumentPrintService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<PrintDocumentResult> RenderDefaultAsync(
        string documentType, string documentId, CancellationToken ct)
    {
        return documentType.ToUpperInvariant() switch
        {
            "PRODUCTIONORDER" => await RenderProductionOrderPdfAsync(documentId, ct),
            _ => throw new NotSupportedException(
                $"Loại tài liệu '{documentType}' chưa được hỗ trợ in mặc định.")
        };
    }

    public async Task<PrintDocumentResult> RenderWithTemplateAsync(
        string documentType, string documentId, int templateId, CancellationToken ct)
    {
        var template = await db.DocumentTemplates.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TemplateId == templateId, ct)
            ?? throw new InvalidOperationException($"Mẫu in #{templateId} không tồn tại.");

        var fields = await BuildFieldDictionaryAsync(documentType, documentId, ct);
        var detailRows = await BuildDetailRowsAsync(documentType, documentId, ct);

        var storageKey = $"templates/{template.FileId}";
        await using var fileStream = await fileStorage.OpenReadAsync(storageKey, ct);

        var bytes = await FillExcelTemplateAsync(fileStream, fields, detailRows, ct);
        var ext = template.OutputFormat == PrintOutputFormat.Excel ? "xlsx" : "pdf";
        var contentType = template.OutputFormat == PrintOutputFormat.Excel
            ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            : "application/pdf";

        return new PrintDocumentResult(bytes, contentType, $"{documentType}_{documentId}.{ext}");
    }

    // ── default PDF renderers ─────────────────────────────────────────────

    private async Task<PrintDocumentResult> RenderProductionOrderPdfAsync(
        string documentId, CancellationToken ct)
    {
        if (!int.TryParse(documentId, out var poid))
            throw new ArgumentException($"ID lệnh sản xuất không hợp lệ: {documentId}");

        var po = await db.ProductionOrders.AsNoTracking()
            .FirstOrDefaultAsync(p => p.POID == poid, ct)
            ?? throw new InvalidOperationException($"Lệnh sản xuất #{poid} không tồn tại.");

        var product = await db.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductCode == po.ProductCode, ct);

        var workOrders = await db.WorkOrders.AsNoTracking()
            .Where(w => w.POID == poid)
            .Take(100)
            .ToListAsync(ct);

        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20, Unit.Millimetre);

                page.Header().Text($"LỆNH SẢN XUẤT — {po.POCode}")
                    .FontSize(16).Bold().AlignCenter();

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(cd =>
                        {
                            cd.RelativeColumn(1);
                            cd.RelativeColumn(2);
                            cd.RelativeColumn(1);
                            cd.RelativeColumn(2);
                        });
                        HeaderCell(t, "Số lệnh SX:", po.POCode);
                        HeaderCell(t, "Mã SP:", po.ProductCode);
                        HeaderCell(t, "Tên SP:", product?.ProductName ?? "—");
                        HeaderCell(t, "SL kế hoạch:", po.TargetQuantity.ToString());
                        HeaderCell(t, "Trạng thái:", po.Status.ToString());
                        HeaderCell(t, "Độ ưu tiên:", po.Priority.ToString());
                        HeaderCell(t, "Ngày bắt đầu:", po.PlannedStartDate?.ToString("dd/MM/yyyy") ?? "—");
                        HeaderCell(t, "Ngày kết thúc:", po.PlannedEndDate?.ToString("dd/MM/yyyy") ?? "—");
                    });

                    if (workOrders.Count > 0)
                    {
                        col.Item().Text("Danh sách lệnh công việc").FontSize(12).Bold();
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(cd =>
                            {
                                cd.ConstantColumn(35);
                                cd.RelativeColumn(2);
                                cd.RelativeColumn(1);
                                cd.RelativeColumn(2);
                            });
                            t.Header(h =>
                            {
                                foreach (var label in new[] { "STT", "Mã WO", "SL", "Trạng thái" })
                                    h.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text(label).Bold();
                            });
                            for (int i = 0; i < workOrders.Count; i++)
                            {
                                var wo = workOrders[i];
                                t.Cell().Padding(4).Text($"{i + 1}");
                                t.Cell().Padding(4).Text(wo.WOCode);
                                t.Cell().Padding(4).Text($"{wo.TargetQuantity.Value}");
                                t.Cell().Padding(4).Text(wo.Status.ToString());
                            }
                        });
                    }
                });

                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text($"In lúc: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8);
                    row.RelativeItem().AlignRight().Text(x =>
                    {
                        x.Span("Trang ").FontSize(8);
                        x.CurrentPageNumber().FontSize(8);
                        x.Span("/").FontSize(8);
                        x.TotalPages().FontSize(8);
                    });
                });
            });
        }).GeneratePdf();

        return new PrintDocumentResult(bytes, "application/pdf", $"LenhSanXuat_{po.POCode}.pdf");
    }

    private static void HeaderCell(TableDescriptor t, string label, string value)
    {
        t.Cell().Padding(4).Text(label).Bold().FontSize(9);
        t.Cell().Padding(4).Text(value).FontSize(9);
    }

    // ── custom Excel template ─────────────────────────────────────────────

    private async Task<Dictionary<string, string>> BuildFieldDictionaryAsync(
        string documentType, string documentId, CancellationToken ct)
    {
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (documentType.Equals("ProductionOrder", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(documentId, out var poid))
        {
            var po = await db.ProductionOrders.AsNoTracking()
                .FirstOrDefaultAsync(p => p.POID == poid, ct);
            if (po is not null)
            {
                var product = await db.Products.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ProductCode == po.ProductCode, ct);

                fields["POCode"] = po.POCode;
                fields["ProductCode"] = po.ProductCode;
                fields["ProductName"] = product?.ProductName ?? string.Empty;
                fields["TargetQuantity"] = po.TargetQuantity.ToString();
                fields["Status"] = po.Status.ToString();
                fields["Priority"] = po.Priority.ToString();
                fields["PlannedStart"] = po.PlannedStartDate?.ToString("dd/MM/yyyy") ?? string.Empty;
                fields["PlannedEnd"] = po.PlannedEndDate?.ToString("dd/MM/yyyy") ?? string.Empty;
                fields["ProductionDeadline"] = po.ProductionDeadline?.ToString("dd/MM/yyyy") ?? string.Empty;
                fields["CreatedAt"] = DateTime.UtcNow.ToString("dd/MM/yyyy");
            }
        }

        return fields;
    }

    private async Task<List<Dictionary<string, string>>> BuildDetailRowsAsync(
        string documentType, string documentId, CancellationToken ct)
    {
        var rows = new List<Dictionary<string, string>>();

        if (documentType.Equals("ProductionOrder", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(documentId, out var poid))
        {
            var workOrders = await db.WorkOrders.AsNoTracking()
                .Where(w => w.POID == poid)
                .Take(200)
                .ToListAsync(ct);

            for (int i = 0; i < workOrders.Count; i++)
            {
                var wo = workOrders[i];
                rows.Add(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["StepNo"] = (i + 1).ToString(),
                    ["Quantity"] = wo.TargetQuantity.Value.ToString(),
                    ["Status"] = wo.Status.ToString(),
                });
            }
        }

        return rows;
    }

    private static async Task<byte[]> FillExcelTemplateAsync(
        Stream templateStream,
        Dictionary<string, string> fields,
        List<Dictionary<string, string>> detailRows,
        CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await templateStream.CopyToAsync(ms, ct);
        ms.Position = 0;

        using var wb = new XLWorkbook(ms);

        foreach (var ws in wb.Worksheets)
        {
            // Locate the ##Detail_ template row
            int detailRowIndex = 0;
            List<(int ColNum, string Template)> detailCellTemplates = [];

            foreach (var row in ws.RowsUsed())
            {
                foreach (var cell in row.CellsUsed())
                {
                    if (cell.Value.ToString().Contains("##Detail_"))
                    {
                        detailRowIndex = row.RowNumber();
                        break;
                    }
                }
                if (detailRowIndex > 0) break;
            }

            if (detailRowIndex > 0)
            {
                var templateRow = ws.Row(detailRowIndex);
                foreach (var cell in templateRow.CellsUsed())
                    detailCellTemplates.Add((cell.Address.ColumnNumber, cell.Value.ToString()));

                if (detailRows.Count == 0)
                {
                    templateRow.Delete();
                }
                else
                {
                    if (detailRows.Count > 1)
                        ws.Row(detailRowIndex + 1).InsertRowsAbove(detailRows.Count - 1);

                    for (int i = 0; i < detailRows.Count; i++)
                    {
                        var targetRow = ws.Row(detailRowIndex + i);
                        foreach (var (col, tmpl) in detailCellTemplates)
                            targetRow.Cell(col).Value = SubstituteDetail(tmpl, detailRows[i]);
                    }
                }
            }

            // Apply header {{...}} substitutions
            foreach (var row in ws.RowsUsed())
            {
                foreach (var cell in row.CellsUsed())
                {
                    var text = cell.Value.ToString();
                    if (text.Contains("{{"))
                        cell.Value = SubstituteHeader(text, fields);
                }
            }
        }

        using var output = new MemoryStream();
        wb.SaveAs(output);
        return output.ToArray();
    }

    private static string SubstituteHeader(string template, Dictionary<string, string> fields)
        => Regex.Replace(template, @"\{\{(\w+)\}\}", m =>
            fields.TryGetValue(m.Groups[1].Value, out var val) ? val : m.Value);

    private static string SubstituteDetail(string template, Dictionary<string, string> row)
        => Regex.Replace(template, @"##Detail_(\w+)", m =>
            row.TryGetValue(m.Groups[1].Value, out var val) ? val : string.Empty);
}
