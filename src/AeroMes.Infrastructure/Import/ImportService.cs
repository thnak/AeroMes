using AeroMes.Application.Import;
using AeroMes.Domain.Master;
using AeroMes.Domain.Production;
using AeroMes.Infrastructure.Data;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AeroMes.Infrastructure.Import;

public sealed class ImportService(AppDbContext db) : IImportService
{
    // ── supported categories ──────────────────────────────────────────────

    private static readonly IReadOnlyList<ImportCategoryInfo> Categories =
    [
        new("Products", "Sản phẩm / Vật liệu",
        [
            new("ProductCode", "Mã sản phẩm", true),
            new("ProductName", "Tên sản phẩm", true),
            new("BaseUoMCode", "Đơn vị tính cơ bản", true),
            new("ItemType", "Loại hàng (Finished/SemiFinished/RawMaterial/Consumable/Service)", false),
            new("CategoryCode", "Mã nhóm hàng", false),
            new("Description", "Mô tả", false),
        ]),
        new("BomItems", "Định mức vật tư (BOM)",
        [
            new("ProductCode", "Mã sản phẩm", true),
            new("MaterialCode", "Mã vật liệu", true),
            new("Quantity", "Số lượng", true),
            new("UomCode", "Đơn vị tính", false),
            new("ScrapRate", "Tỷ lệ phế (%)", false),
        ]),
        new("BeginningInventory", "Số dư đầu kỳ",
        [
            new("ProductCode", "Mã sản phẩm / vật tư", true),
            new("LocationCode", "Mã vị trí kho", true),
            new("LotNumber", "Số lô", false),
            new("Quantity", "Số lượng", true),
        ]),
    ];

    public IReadOnlyList<ImportCategoryInfo> GetSupportedCategories() => Categories;

    // ── template generation ───────────────────────────────────────────────

    public Task<byte[]> GetTemplateAsync(string category, CancellationToken ct)
    {
        var cat = Categories.FirstOrDefault(c =>
            c.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        if (cat is null) return Task.FromResult(Array.Empty<byte>());

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Data");

        // Write header row
        for (int i = 0; i < cat.Columns.Count; i++)
        {
            var col = cat.Columns[i];
            var cell = ws.Cell(1, i + 1);
            cell.Value = col.IsRequired ? $"{col.Name}*" : col.Name;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
        }

        // Write description row
        var descWs = wb.Worksheets.Add("Hướng dẫn");
        descWs.Cell(1, 1).Value = "Tên cột";
        descWs.Cell(1, 2).Value = "Mô tả";
        descWs.Cell(1, 3).Value = "Bắt buộc";
        descWs.Cell(1, 1).Style.Font.Bold = true;
        descWs.Cell(1, 2).Style.Font.Bold = true;
        descWs.Cell(1, 3).Style.Font.Bold = true;

        for (int i = 0; i < cat.Columns.Count; i++)
        {
            var col = cat.Columns[i];
            descWs.Cell(i + 2, 1).Value = col.Name;
            descWs.Cell(i + 2, 2).Value = col.Description;
            descWs.Cell(i + 2, 3).Value = col.IsRequired ? "Có" : "Không";
        }

        descWs.Column(1).AdjustToContents();
        descWs.Column(2).AdjustToContents();
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return Task.FromResult(ms.ToArray());
    }

    // ── validation ────────────────────────────────────────────────────────

    public async Task<ImportValidationResult> ValidateAsync(
        Stream fileStream,
        string fileName,
        string category,
        int startRow,
        CancellationToken ct)
    {
        using var wb = new XLWorkbook(fileStream);
        var ws = wb.Worksheets.First();

        if (startRow < 1) startRow = 1;
        var headerRow = startRow;
        var dataStartRow = startRow + 1;

        // Read header
        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var lastCol = ws.Row(headerRow).LastCellUsed()?.Address.ColumnNumber ?? 0;
        for (int col = 1; col <= lastCol; col++)
        {
            var val = ws.Cell(headerRow, col).GetString().Trim().TrimEnd('*');
            if (!string.IsNullOrEmpty(val))
                headers[val] = col;
        }

        var validRows = new List<Dictionary<string, string?>>();
        var errorRows = new List<ImportRowError>();
        int totalRows = 0;

        // Load existing codes for dedup check
        HashSet<string> existingProducts = category.Equals("Products", StringComparison.OrdinalIgnoreCase)
            ? (await db.Products.AsNoTracking().Select(p => p.ProductCode).ToListAsync(ct))
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
            : [];

        HashSet<string> existingLocations = category.Equals("BeginningInventory", StringComparison.OrdinalIgnoreCase)
            ? (await db.StorageLocations.AsNoTracking().Select(l => l.LocationCode).ToListAsync(ct))
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
            : [];

        for (int row = dataStartRow; ws.Row(row).CellsUsed().Any(); row++)
        {
            totalRows++;
            var errors = new List<string>();
            var fields = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            foreach (var (colName, colIdx) in headers)
                fields[colName] = ws.Cell(row, colIdx).GetString().Trim().NullIfEmpty();

            ValidateRow(category, row, fields, existingProducts, existingLocations, errors);

            if (errors.Count > 0)
                errorRows.Add(new ImportRowError(row, [.. errors]));
            else
                validRows.Add(fields);
        }

        var validJson = validRows.Count > 0 ? SerializeRows(validRows) : null;
        var errorJson = errorRows.Count > 0 ? SerializeErrors(errorRows) : null;

        return new ImportValidationResult(
            category, fileName, totalRows,
            validRows.Count, errorRows.Count,
            errorRows, validJson, errorJson);
    }

    private static void ValidateRow(
        string category,
        int rowNum,
        Dictionary<string, string?> fields,
        HashSet<string> existingProducts,
        HashSet<string> existingLocations,
        List<string> errors)
    {
        if (category.Equals("Products", StringComparison.OrdinalIgnoreCase))
        {
            Required(fields, "ProductCode", errors, rowNum);
            Required(fields, "ProductName", errors, rowNum);
            Required(fields, "BaseUoMCode", errors, rowNum);
            if (fields.TryGetValue("ProductCode", out var code) && code is not null
                && existingProducts.Contains(code))
                errors.Add($"Dòng {rowNum}: Mã sản phẩm '{code}' đã tồn tại trong hệ thống (bỏ qua).");
        }
        else if (category.Equals("BomItems", StringComparison.OrdinalIgnoreCase))
        {
            Required(fields, "ProductCode", errors, rowNum);
            Required(fields, "MaterialCode", errors, rowNum);
            if (fields.TryGetValue("Quantity", out var qtyStr))
            {
                if (!decimal.TryParse(qtyStr, out var qty) || qty <= 0)
                    errors.Add($"Dòng {rowNum}: Số lượng phải là số dương.");
            }
            else errors.Add($"Dòng {rowNum}: Thiếu cột 'Quantity'.");
        }
        else if (category.Equals("BeginningInventory", StringComparison.OrdinalIgnoreCase))
        {
            Required(fields, "ProductCode", errors, rowNum);
            Required(fields, "LocationCode", errors, rowNum);
            if (fields.TryGetValue("LocationCode", out var loc) && loc is not null
                && !existingLocations.Contains(loc))
                errors.Add($"Dòng {rowNum}: Vị trí kho '{loc}' không tồn tại.");
            if (!fields.TryGetValue("Quantity", out var qStr) ||
                !decimal.TryParse(qStr, out var q) || q < 0)
                errors.Add($"Dòng {rowNum}: Số lượng không hợp lệ.");
        }
    }

    private static void Required(Dictionary<string, string?> fields, string key, List<string> errors, int row)
    {
        if (!fields.TryGetValue(key, out var val) || string.IsNullOrWhiteSpace(val))
            errors.Add($"Dòng {row}: Thiếu trường bắt buộc '{key}'.");
    }

    // ── execution ─────────────────────────────────────────────────────────

    public async Task<int> ExecuteAsync(string category, string validRowsJson, CancellationToken ct)
    {
        var rows = JsonSerializer.Deserialize<List<Dictionary<string, string?>>>(validRowsJson)
            ?? [];

        if (category.Equals("Products", StringComparison.OrdinalIgnoreCase))
            return await ExecuteProductsAsync(rows, ct);

        if (category.Equals("BomItems", StringComparison.OrdinalIgnoreCase))
            return await ExecuteBomItemsAsync(rows, ct);

        if (category.Equals("BeginningInventory", StringComparison.OrdinalIgnoreCase))
            return await ExecuteBeginningInventoryAsync(rows, ct);

        throw new NotSupportedException($"Danh mục '{category}' không được hỗ trợ.");
    }

    private async Task<int> ExecuteProductsAsync(
        List<Dictionary<string, string?>> rows, CancellationToken ct)
    {
        var existing = (await db.Products.AsNoTracking()
            .Select(p => p.ProductCode).ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        int count = 0;
        foreach (var row in rows)
        {
            var code = row.GetValueOrDefault("ProductCode")?.Trim().ToUpperInvariant();
            if (code is null || existing.Contains(code)) continue;

            var name = row.GetValueOrDefault("ProductName") ?? code;
            var uom = row.GetValueOrDefault("BaseUoMCode") ?? "EA";
            var itemTypeStr = row.GetValueOrDefault("ItemType") ?? "RM";

            if (!Enum.TryParse<ItemType>(itemTypeStr, true, out var itemType))
                itemType = ItemType.RM;

            var product = Product.Create(code, name, uom, itemType,
                null, null, false, false, null, null, null, null, null, "import");
            db.Products.Add(product);
            existing.Add(code);
            count++;
        }

        if (count > 0) await db.SaveChangesAsync(ct);
        return count;
    }

    private async Task<int> ExecuteBomItemsAsync(
        List<Dictionary<string, string?>> rows, CancellationToken ct)
    {
        int count = 0;
        foreach (var row in rows)
        {
            var productCode = row.GetValueOrDefault("ProductCode")?.Trim().ToUpperInvariant();
            var materialCode = row.GetValueOrDefault("MaterialCode")?.Trim().ToUpperInvariant();
            if (productCode is null || materialCode is null) continue;

            _ = decimal.TryParse(row.GetValueOrDefault("Quantity"), out var qty);
            if (qty <= 0) qty = 1;
            _ = decimal.TryParse(row.GetValueOrDefault("ScrapRate"), out var scrap);
            var uom = row.GetValueOrDefault("UomCode") ?? "EA";

            var exists = await db.BomItems.AsNoTracking()
                .AnyAsync(b => b.ParentProductCode == productCode && b.ChildProductCode == materialCode, ct);
            if (exists) continue;

            var bom = BomItem.Create(productCode, materialCode, qty, scrap, "import");
            db.BomItems.Add(bom);
            count++;
        }
        if (count > 0) await db.SaveChangesAsync(ct);
        return count;
    }

    private async Task<int> ExecuteBeginningInventoryAsync(
        List<Dictionary<string, string?>> rows, CancellationToken ct)
    {
        var locations = await db.StorageLocations.AsNoTracking()
            .ToDictionaryAsync(l => l.LocationCode, l => l.LocationID, StringComparer.OrdinalIgnoreCase, ct);

        int count = 0;
        foreach (var row in rows)
        {
            var productCode = row.GetValueOrDefault("ProductCode")?.Trim().ToUpperInvariant();
            var locationCode = row.GetValueOrDefault("LocationCode")?.Trim();
            if (productCode is null || locationCode is null) continue;
            if (!locations.TryGetValue(locationCode, out var locationId)) continue;

            _ = decimal.TryParse(row.GetValueOrDefault("Quantity"), out var qty);
            var lot = row.GetValueOrDefault("LotNumber") ?? "INIT";

            var exists = await db.InventoryStocks.AsNoTracking()
                .AnyAsync(s => s.LocationID == locationId &&
                               s.ProductCode == productCode &&
                               s.LotNumber == lot, ct);
            if (exists) continue;

            var stock = InventoryStock.Create(locationId, productCode, lot, qty);
            db.InventoryStocks.Add(stock);
            count++;
        }
        if (count > 0) await db.SaveChangesAsync(ct);
        return count;
    }

    // ── error report ──────────────────────────────────────────────────────

    public Task<byte[]> GetErrorReportAsync(string errorRowsJson, CancellationToken ct)
    {
        var errors = JsonSerializer.Deserialize<List<ImportRowError>>(errorRowsJson) ?? [];

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Lỗi");

        ws.Cell(1, 1).Value = "Dòng";
        ws.Cell(1, 2).Value = "Danh sách lỗi";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 2).Style.Font.Bold = true;

        for (int i = 0; i < errors.Count; i++)
        {
            ws.Cell(i + 2, 1).Value = errors[i].RowNumber;
            ws.Cell(i + 2, 2).Value = string.Join("; ", errors[i].Errors);
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return Task.FromResult(ms.ToArray());
    }

    // ── helpers ───────────────────────────────────────────────────────────

    private static string SerializeRows(List<Dictionary<string, string?>> rows)
        => JsonSerializer.Serialize(rows);

    private static string SerializeErrors(List<ImportRowError> errors)
        => JsonSerializer.Serialize(errors);
}

file static class StringExtensions
{
    public static string? NullIfEmpty(this string? s)
        => string.IsNullOrWhiteSpace(s) ? null : s;
}
