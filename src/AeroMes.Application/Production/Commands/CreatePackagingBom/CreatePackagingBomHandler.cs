using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.CreatePackagingBom;

public class CreatePackagingBomHandler(
    IPackagingRepository repo,
    IValidator<CreatePackagingBomCommand> validator)
    : ICommandHandler<CreatePackagingBomCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreatePackagingBomCommand cmd, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(cmd, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        var bom = PackagingBom.Create(cmd.ProductCode, cmd.Notes);
        foreach (var line in cmd.Lines)
            bom.AddLine(line.MaterialCode, line.Quantity, line.UnitCode, line.Notes);

        await repo.AddBomAsync(bom, ct);
        await repo.SaveChangesAsync(ct);
        return ValidationResult<int>.Ok(bom.PackagingBomID);
    }
}
