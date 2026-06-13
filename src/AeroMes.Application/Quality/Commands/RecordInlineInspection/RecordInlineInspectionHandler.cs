using AeroMes.Application.Common;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.Commands.RecordInlineInspection;

public class RecordInlineInspectionHandler(
    IInlineInspectionRepository repo,
    IValidator<RecordInlineInspectionCommand> validator)
    : ICommandHandler<RecordInlineInspectionCommand, ValidationResult<long>>
{
    public async Task<ValidationResult<long>> HandleAsync(
        RecordInlineInspectionCommand cmd, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(cmd, ct);
        if (!v.IsValid) return ValidationResult<long>.Invalid(v.ToErrorDictionary());

        var defects = cmd.Defects.Select(d =>
            (d.DefectCode, d.Quantity, d.DefectLocation, d.IsMajor));

        var inspection = InlineInspection.Create(
            cmd.WOID, cmd.WorkCenterID, cmd.StyleCode, cmd.ColorCode,
            cmd.InspectorID, cmd.ShiftCode, cmd.SampleSize,
            defects, cmd.DHU_Target, cmd.Notes);

        await repo.AddAsync(inspection, ct);
        await repo.SaveChangesAsync(ct);
        return ValidationResult<long>.Ok(inspection.InspectionID);
    }
}
