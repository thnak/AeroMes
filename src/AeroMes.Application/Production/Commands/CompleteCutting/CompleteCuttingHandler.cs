using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.CompleteCutting;

public class CompleteCuttingHandler(
    ICutOrderRepository repo,
    IValidator<CompleteCuttingCommand> validator) : ICommandHandler<CompleteCuttingCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(CompleteCuttingCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<Unit>.Invalid(vr.ToErrorDictionary());

        var cutOrder = await repo.GetByIdAsync(cmd.CutOrderID, ct);
        if (cutOrder is null)
            return ValidationResult<Unit>.NotFound($"Cut order {cmd.CutOrderID} not found.");

        try
        {
            cutOrder.CompleteCutting(
                cmd.ActualFabricMeters,
                cmd.MarkerEfficiencyPct,
                cmd.Lines.Select(l => (l.SizeCode, l.QuantityCut)));

            var bundles = GenerateBundles(cutOrder, cmd.Lines, cmd.BundleSize);
            await repo.AddBundlesAsync(bundles, ct);
            await repo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }

    private static IEnumerable<Bundle> GenerateBundles(
        CutOrder cutOrder, IReadOnlyList<CompleteCuttingLineInput> lines, int bundleSize)
    {
        foreach (var line in lines.Where(l => l.QuantityCut > 0))
        {
            var remaining = line.QuantityCut;
            var bundleNum = 1;
            while (remaining > 0)
            {
                var pcs = Math.Min(remaining, bundleSize);
                var barcode = $"B-{cutOrder.CutOrderID}-{line.SizeCode}-{bundleNum:D4}";
                yield return Bundle.Create(
                    cutOrder.CutOrderID, cutOrder.StyleCode, cutOrder.ColorCode,
                    line.SizeCode, bundleNum, pcs, barcode);
                bundleNum++;
                remaining -= pcs;
            }
        }
    }
}
