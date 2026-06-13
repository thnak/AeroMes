using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteMaterialRequisition;

public class DeleteMaterialRequisitionHandler(
    IMaterialRequisitionRepository requisitionRepo,
    IUnitOfWork uow)
    : ICommandHandler<DeleteMaterialRequisitionCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteMaterialRequisitionCommand cmd, CancellationToken ct)
    {
        var requisition = await requisitionRepo.GetByIdAsync(cmd.RequisitionId, ct);
        if (requisition is null)
            return ValidationResult<Unit>.NotFound($"Yêu cầu xuất '{cmd.RequisitionId}' không tồn tại.");

        if (requisition.Status != MaterialRequisitionStatus.Draft)
            return ValidationResult<Unit>.Failure(
                $"Chỉ có thể xóa yêu cầu xuất ở trạng thái Nháp. Trạng thái hiện tại: {requisition.Status}.");

        try
        {
            requisition.SoftDelete(cmd.DeletedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
