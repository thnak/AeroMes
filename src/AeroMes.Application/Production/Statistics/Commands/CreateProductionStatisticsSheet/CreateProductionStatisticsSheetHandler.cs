using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Statistics.Commands.CreateProductionStatisticsSheet;

public sealed class CreateProductionStatisticsSheetHandler(
    IProductionStatisticsSheetRepository repo,
    IValidator<CreateProductionStatisticsSheetCommand> validator,
    IUnitOfWork uow) : ICommandHandler<CreateProductionStatisticsSheetCommand, ValidationResult<StatisticsSheetCreatedResult>>
{
    public async Task<ValidationResult<StatisticsSheetCreatedResult>> HandleAsync(
        CreateProductionStatisticsSheetCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<StatisticsSheetCreatedResult>.Invalid(vr.ToErrorDictionary());

        var count = await repo.CountAsync(ct);
        var year = DateTime.UtcNow.Year;
        var sheetNumber = $"PSS-{year}-{count + 1:D4}";

        var sheet = ProductionStatisticsSheet.Create(
            sheetNumber, cmd.SheetType, cmd.POID, cmd.MPOId,
            cmd.ProductionDate, cmd.Notes, cmd.CreatedBy);

        foreach (var ol in cmd.OutputLines)
            sheet.AddOutputLine(ol.ProductCode, ol.PlannedQty, ol.QualifiedQty, ol.DefectiveQty, ol.DefectCodeId);

        foreach (var ml in cmd.MaterialLines)
            sheet.AddMaterialLine(ml.MaterialCode, ml.BomStandardQty, ml.ActualUsedQty, ml.UoMCode, ml.VarianceReason);

        foreach (var bp in cmd.ByProductLines)
            sheet.AddByProductLine(bp.ProductCode, bp.Qty, bp.UoMCode, bp.WarehouseCode);

        await repo.AddAsync(sheet, ct);
        await uow.SaveChangesAsync(ct);

        return ValidationResult<StatisticsSheetCreatedResult>.Ok(
            new StatisticsSheetCreatedResult(sheet.SheetId, sheet.SheetNumber));
    }
}
