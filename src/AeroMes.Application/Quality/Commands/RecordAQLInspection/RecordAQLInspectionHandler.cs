using AeroMes.Application.Common;
using AeroMes.Application.Quality.Services;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.Commands.RecordAQLInspection;

public class RecordAQLInspectionHandler(
    IAQLInspectionRepository repo,
    IAQLSamplingTableService samplingTable,
    IValidator<RecordAQLInspectionCommand> validator)
    : ICommandHandler<RecordAQLInspectionCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        RecordAQLInspectionCommand cmd, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(cmd, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        AQLSamplingPlan plan;
        try
        {
            plan = samplingTable.GetSamplingPlan(cmd.LotSize, cmd.AQLLevel, cmd.InspectionLevel);
        }
        catch (ArgumentException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }

        var defects = cmd.Defects.Select(d => (d.DefectCode, d.Quantity, d.IsMajor));
        var inspection = AQLInspection.Create(
            cmd.WOID, cmd.AQLLevel, cmd.InspectionLevel,
            cmd.LotSize, plan.SampleSize, plan.AcceptanceNumber, plan.RejectionNumber,
            cmd.InspectorID, defects, cmd.Notes);

        await repo.AddAsync(inspection, ct);
        await repo.SaveChangesAsync(ct);
        return ValidationResult<int>.Ok(inspection.AQLInspectionID);
    }
}
