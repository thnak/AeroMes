using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Master;
using AeroMes.Domain.Master.Repositories;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.FabricRolls.Commands.RegisterFabricRoll;

public class RegisterFabricRollHandler(
    IFabricRollRepository rollRepo,
    IInventoryStockRepository stockRepo,
    IUnitOfWork uow,
    IValidator<RegisterFabricRollCommand> validator) : ICommandHandler<RegisterFabricRollCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(RegisterFabricRollCommand cmd, CancellationToken ct)
    {
        var vr = await validator.ValidateAsync(cmd, ct);
        if (!vr.IsValid) return ValidationResult<int>.Invalid(vr.ToErrorDictionary());

        if (await rollRepo.BarcodeExistsAsync(cmd.RollBarcode, ct))
            return ValidationResult<int>.Invalid(new Dictionary<string, string[]>
            {
                ["RollBarcode"] = ["Barcode already registered."],
            });

        var roll = FabricRoll.Register(
            cmd.RollBarcode,
            cmd.FabricProductCode,
            cmd.LotNumber,
            cmd.ShadeCode,
            cmd.GrossLengthMeters,
            cmd.GrossWeightKg,
            cmd.FabricWidthCm,
            cmd.SupplierCode,
            cmd.LocationID);

        await rollRepo.AddAsync(roll, ct);

        if (cmd.LocationID.HasValue)
        {
            var existing = await stockRepo.FindByKeyAsync(cmd.LocationID.Value, cmd.FabricProductCode, cmd.LotNumber, ct);
            if (existing is null)
            {
                var stock = InventoryStock.Create(
                    cmd.LocationID.Value,
                    cmd.FabricProductCode,
                    cmd.LotNumber,
                    cmd.GrossLengthMeters,
                    cmd.GrossWeightKg);
                await stockRepo.AddAsync(stock, ct);
            }
            else
            {
                existing.Adjust(cmd.GrossLengthMeters, cmd.GrossWeightKg);
            }
        }

        await uow.SaveChangesAsync(ct);
        return ValidationResult<int>.Ok(roll.RollID);
    }
}
