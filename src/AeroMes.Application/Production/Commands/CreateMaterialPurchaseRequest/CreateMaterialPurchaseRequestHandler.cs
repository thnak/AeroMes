using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using AeroMes.Domain.Production.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.CreateMaterialPurchaseRequest;

public class CreateMaterialPurchaseRequestHandler(
    IMaterialPurchaseRequestRepository repo,
    IValidator<CreateMaterialPurchaseRequestCommand> validator)
    : ICommandHandler<CreateMaterialPurchaseRequestCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        CreateMaterialPurchaseRequestCommand cmd, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(cmd, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        var number = $"PR-{DateTime.UtcNow:yyyyMMddHHmmss}";
        if (await repo.NumberExistsAsync(number, ct))
            number += "-1";

        var request = MaterialPurchaseRequest.Create(
            number, cmd.Requestor, cmd.RequestingUnit,
            cmd.Deadline, cmd.ProcurementPurpose,
            cmd.SourceType, cmd.SourceReferenceId, cmd.SalesOrderCode, null);

        foreach (var l in cmd.Lines)
        {
            var line = request.AddLine(l.MaterialCode, l.MaterialName, l.UnitOfMeasure, l.RequiredQty, null);
            if (l.Length.HasValue || l.Width.HasValue || l.Height.HasValue
                || l.Radius.HasValue || l.Weight.HasValue)
                line.SetDimensions(l.Length, l.Width, l.Height, l.Radius, l.Weight);
        }

        await repo.AddAsync(request, ct);
        await repo.SaveChangesAsync(ct);
        return ValidationResult<int>.Ok(request.RequestID);
    }
}
