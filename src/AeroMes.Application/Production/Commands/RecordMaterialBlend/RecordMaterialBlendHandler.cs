using System.Text.Json;
using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.RecordMaterialBlend;

public class RecordMaterialBlendHandler(
    IMaterialBlendLogRepository repo,
    IProductRepository productRepo,
    IValidator<RecordMaterialBlendCommand> validator) : ICommandHandler<RecordMaterialBlendCommand, ValidationResult<long>>
{
    public async Task<ValidationResult<long>> HandleAsync(RecordMaterialBlendCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<long>.Invalid(vr.ToErrorDictionary());

        try
        {
            var product = await productRepo.GetByCodeAsync(cmd.ResinProductCode, ct);
            if (product is null)
                return ValidationResult<long>.NotFound($"Product '{cmd.ResinProductCode}' not found.");

            // Read max_regrind_pct from CustomAttributes JSON
            decimal maxRegrindPct = 0m;
            if (!string.IsNullOrWhiteSpace(product.CustomAttributes))
            {
                try
                {
                    using var doc = JsonDocument.Parse(product.CustomAttributes);
                    if (doc.RootElement.TryGetProperty("max_regrind_pct", out var pctProp))
                        maxRegrindPct = pctProp.GetDecimal();
                }
                catch (JsonException)
                {
                    // If JSON is malformed, default to 0 (no regrind allowed)
                }
            }

            var blendLog = MaterialBlendLog.Record(
                cmd.JobID, cmd.ResinProductCode, cmd.VirginLotNumber, cmd.VirginQtyKg,
                cmd.RegrindLotNumber, cmd.RegrindQtyKg, maxRegrindPct);

            await repo.AddAsync(blendLog, ct);
            await repo.SaveChangesAsync(ct);

            // If non-compliant, return 422 so the caller knows QA approval is needed
            if (!blendLog.IsCompliant)
            {
                return ValidationResult<long>.Failure(
                    $"BLEND_RATIO_EXCEEDED: Regrind ratio {blendLog.RegrindRatioPct:F2}% exceeds maximum {maxRegrindPct:F2}%. BlendLogID={blendLog.BlendLogID}. QA approval required via POST /api/material-blends/{blendLog.BlendLogID}/approve");
            }

            return ValidationResult<long>.Ok(blendLog.BlendLogID);
        }
        catch (DomainException ex)
        {
            return ValidationResult<long>.Failure(ex.Message);
        }
    }
}
