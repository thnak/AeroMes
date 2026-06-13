using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.FulfillMaterialRequisition;

public class FulfillMaterialRequisitionHandler(
    IMaterialRequisitionRepository requisitionRepo,
    IUnitOfWork uow,
    IValidator<FulfillMaterialRequisitionCommand> validator)
    : ICommandHandler<FulfillMaterialRequisitionCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        FulfillMaterialRequisitionCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return ValidationResult<Unit>.Invalid(validation.ToErrorDictionary());

        var requisition = await requisitionRepo.GetByIdWithLinesAsync(cmd.RequisitionId, ct);
        if (requisition is null)
            return ValidationResult<Unit>.NotFound($"Yêu cầu xuất '{cmd.RequisitionId}' không tồn tại.");

        var knownLineIds = requisition.Lines.Select(l => l.LineId).ToHashSet();
        foreach (var input in cmd.IssuanceLines)
        {
            if (!knownLineIds.Contains(input.LineId))
                return ValidationResult<Unit>.NotFound(
                    $"Dòng vật tư '{input.LineId}' không thuộc yêu cầu xuất này.");
        }

        try
        {
            var actualQtyByLineId = cmd.IssuanceLines.ToDictionary(l => l.LineId, l => l.ActualIssuedQuantity);
            requisition.Fulfill(actualQtyByLineId, cmd.FulfilledBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
