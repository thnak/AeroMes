using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.CreateBundlesFromCutOrder;

public class CreateBundlesFromCutOrderHandler(
    ICutOrderRepository cutOrderRepo,
    IBundleRepository bundleRepo,
    IValidator<CreateBundlesFromCutOrderCommand> validator)
    : ICommandHandler<CreateBundlesFromCutOrderCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateBundlesFromCutOrderCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<int>.Invalid(vr.ToErrorDictionary());

        var cutOrder = await cutOrderRepo.GetByIdAsync(cmd.CutOrderID, ct);
        if (cutOrder is null)
            return ValidationResult<int>.NotFound($"Cut order {cmd.CutOrderID} not found.");

        var bundles = new List<Bundle>();
        foreach (var line in cutOrder.Lines.Where(l => l.QuantityToCut > 0))
        {
            var remaining = line.QuantityToCut;
            var bundleNum = 1;
            while (remaining > 0)
            {
                var pcs = Math.Min(remaining, cmd.BundleSizePerBundle);
                var barcode = $"B-{cmd.CutOrderID}-{line.SizeCode}-{bundleNum:D4}";
                bundles.Add(Bundle.Create(
                    cmd.CutOrderID, cutOrder.StyleCode, cutOrder.ColorCode,
                    line.SizeCode, bundleNum, pcs, barcode));
                bundleNum++;
                remaining -= pcs;
            }
        }

        await bundleRepo.AddRangeAsync(bundles, ct);
        await bundleRepo.SaveChangesAsync(ct);
        return ValidationResult<int>.Ok(bundles.Count);
    }
}
