using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Wms.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.DeleteFactoryWarehouseExport;

public class DeleteFactoryWarehouseExportHandler(
    IFactoryWarehouseExportRepository exportRepo,
    IUnitOfWork uow)
    : ICommandHandler<DeleteFactoryWarehouseExportCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteFactoryWarehouseExportCommand cmd, CancellationToken ct)
    {
        var export = await exportRepo.GetByIdAsync(cmd.ExportId, ct);
        if (export is null)
            return ValidationResult<Unit>.NotFound($"Phiếu xuất '{cmd.ExportId}' không tồn tại.");

        try
        {
            export.SoftDelete(cmd.DeletedBy);
            await uow.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
