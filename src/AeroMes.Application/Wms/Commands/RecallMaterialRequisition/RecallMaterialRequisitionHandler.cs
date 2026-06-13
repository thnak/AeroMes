using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.RecallMaterialRequisition;

public class RecallMaterialRequisitionHandler(
    IMaterialRequisitionRepository requisitionRepo,
    IUnitOfWork uow)
    : ICommandHandler<RecallMaterialRequisitionCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        RecallMaterialRequisitionCommand cmd, CancellationToken ct)
    {
        var requisition = await requisitionRepo.GetByIdAsync(cmd.RequisitionId, ct);
        if (requisition is null)
            return ValidationResult<Unit>.NotFound($"Yêu cầu xuất '{cmd.RequisitionId}' không tồn tại.");

        try
        {
            requisition.Recall(cmd.RecalledBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
