using AeroMes.Domain.Templates;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Templates.Queries.GetFieldMapping;

public sealed class GetFieldMappingHandler
    : IQueryHandler<GetFieldMappingQuery, IReadOnlyList<TemplateFieldItem>>
{
    private static readonly IReadOnlyList<TemplateFieldItem> ProductionOrderFields =
    [
        new("{{POCode}}", "Số lệnh sản xuất", false, true),
        new("{{ProductCode}}", "Mã sản phẩm", false, true),
        new("{{ProductName}}", "Tên sản phẩm", false, true),
        new("{{TargetQuantity}}", "Số lượng kế hoạch", false, true),
        new("{{Unit}}", "Đơn vị tính", false, false),
        new("{{PlannedStart}}", "Ngày bắt đầu kế hoạch", false, false),
        new("{{PlannedEnd}}", "Ngày kết thúc kế hoạch", false, false),
        new("{{ProductionDeadline}}", "Hạn hoàn thành", false, false),
        new("{{Priority}}", "Độ ưu tiên", false, false),
        new("{{Status}}", "Trạng thái", false, true),
        new("{{Notes}}", "Ghi chú", false, false),
        new("{{CreatedBy}}", "Người tạo", false, false),
        new("{{CreatedAt}}", "Ngày tạo", false, false),
        new("##Detail_StepNo", "STT bước sản xuất (dòng lặp)", true, false),
        new("##Detail_OperationCode", "Mã công đoạn (dòng lặp)", true, false),
        new("##Detail_OperationName", "Tên công đoạn (dòng lặp)", true, false),
        new("##Detail_WorkCenterCode", "Mã trung tâm làm việc (dòng lặp)", true, false),
        new("##Detail_WorkCenterName", "Tên trung tâm làm việc (dòng lặp)", true, false),
    ];

    private static readonly IReadOnlyList<TemplateFieldItem> QualityInspectionFields =
    [
        new("{{InspectionCode}}", "Mã phiếu kiểm tra", false, true),
        new("{{WorkOrderCode}}", "Mã lệnh sản xuất", false, true),
        new("{{ProductCode}}", "Mã sản phẩm", false, true),
        new("{{ProductName}}", "Tên sản phẩm", false, true),
        new("{{InspectedQty}}", "Số lượng kiểm tra", false, true),
        new("{{PassQty}}", "Số lượng đạt", false, true),
        new("{{FailQty}}", "Số lượng không đạt", false, true),
        new("{{DefectQty}}", "Số lượng lỗi", false, false),
        new("{{InspectionDate}}", "Ngày kiểm tra", false, false),
        new("{{Inspector}}", "Người kiểm tra", false, false),
        new("{{Notes}}", "Ghi chú", false, false),
        new("##Detail_CriterionCode", "Mã tiêu chí (dòng lặp)", true, false),
        new("##Detail_CriterionName", "Tên tiêu chí (dòng lặp)", true, false),
        new("##Detail_Standard", "Giá trị chuẩn (dòng lặp)", true, false),
        new("##Detail_Result", "Kết quả (dòng lặp)", true, false),
        new("##Detail_Passed", "Đạt/Không đạt (dòng lặp)", true, false),
    ];

    private static readonly IReadOnlyList<TemplateFieldItem> MaterialStandardFields =
    [
        new("{{MaterialCode}}", "Mã nguyên vật liệu", false, true),
        new("{{MaterialName}}", "Tên nguyên vật liệu", false, true),
        new("{{Unit}}", "Đơn vị tính", false, true),
        new("{{Category}}", "Nhóm hàng", false, false),
        new("{{Description}}", "Mô tả", false, false),
        new("{{CreatedBy}}", "Người tạo", false, false),
        new("{{CreatedAt}}", "Ngày tạo", false, false),
    ];

    private static readonly IReadOnlyList<TemplateFieldItem> MaterialStandardStructureFields =
    [
        new("{{ProductCode}}", "Mã sản phẩm", false, true),
        new("{{ProductName}}", "Tên sản phẩm", false, true),
        new("{{BomVersion}}", "Phiên bản BOM", false, false),
        new("{{EffectiveDate}}", "Ngày hiệu lực", false, false),
        new("{{CreatedBy}}", "Người tạo", false, false),
        new("{{CreatedAt}}", "Ngày tạo", false, false),
        new("##Detail_LineNo", "STT (dòng lặp)", true, false),
        new("##Detail_MaterialCode", "Mã nguyên vật liệu (dòng lặp)", true, true),
        new("##Detail_MaterialName", "Tên nguyên vật liệu (dòng lặp)", true, true),
        new("##Detail_Quantity", "Số lượng (dòng lặp)", true, true),
        new("##Detail_Unit", "Đơn vị (dòng lặp)", true, false),
        new("##Detail_ScrapRate", "Tỷ lệ phế (%%) (dòng lặp)", true, false),
    ];

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<TemplateFieldItem>> FieldMap =
        new Dictionary<string, IReadOnlyList<TemplateFieldItem>>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(DocumentType.ProductionOrder)] = ProductionOrderFields,
            [nameof(DocumentType.QualityInspectionForm)] = QualityInspectionFields,
            [nameof(DocumentType.MaterialStandard)] = MaterialStandardFields,
            [nameof(DocumentType.MaterialStandardStructure)] = MaterialStandardStructureFields,
        };

    public Task<IReadOnlyList<TemplateFieldItem>> HandleAsync(
        GetFieldMappingQuery query, CancellationToken ct = default)
    {
        IReadOnlyList<TemplateFieldItem> result =
            FieldMap.TryGetValue(query.DocumentType, out var fields) ? fields : [];
        return Task.FromResult(result);
    }
}
