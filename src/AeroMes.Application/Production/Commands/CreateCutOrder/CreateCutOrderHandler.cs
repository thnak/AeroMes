using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.CreateCutOrder;

public class CreateCutOrderHandler(
    ICutOrderRepository repo,
    IValidator<CreateCutOrderCommand> validator) : ICommandHandler<CreateCutOrderCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateCutOrderCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<int>.Invalid(vr.ToErrorDictionary());

        if (await repo.CodeExistsAsync(cmd.CutOrderCode, ct))
            return ValidationResult<int>.Invalid(new Dictionary<string, string[]>
            {
                ["CutOrderCode"] = ["Cut order code already exists."],
            });

        try
        {
            var cutOrder = CutOrder.Create(
                cmd.CutOrderCode, cmd.WOID, cmd.StyleCode, cmd.ColorCode,
                cmd.FabricProductCode, cmd.ShadeCode, cmd.PlyCount,
                cmd.SpreadLengthMeters, cmd.FabricWidthCm,
                cmd.Lines.Select(l => (l.SizeCode, l.QuantityToCut)),
                cmd.MarkerReference, cmd.EstimatedFabricMeters);

            await repo.AddAsync(cutOrder, ct);
            await repo.SaveChangesAsync(ct);
            return ValidationResult<int>.Ok(cutOrder.CutOrderID);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
