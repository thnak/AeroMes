using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Quality;
using AeroMes.Domain.Quality.Repositories;
using FluentValidation;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionVouchers.Commands.CreateInspectionVoucher;

public class CreateInspectionVoucherHandler(
    IQualityInspectionVoucherRepository repository,
    IUnitOfWork uow,
    IValidator<CreateInspectionVoucherCommand> validator)
    : ICommandHandler<CreateInspectionVoucherCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(CreateInspectionVoucherCommand command, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(command, ct);
        if (!v.IsValid) return ValidationResult<int>.Invalid(v.ToErrorDictionary());

        var voucher = QualityInspectionVoucher.Create(
            command.VoucherNumber, command.VoucherName,
            command.InspectionType, command.InspectorName,
            command.InspectionDate, command.LinkedRequestId,
            command.ProductionOrderId, command.SampleQuantity,
            command.CreatedBy);

        var id = await repository.AddAsync(voucher, ct);
        await uow.SaveChangesAsync(ct);
        return ValidationResult<int>.Ok(id);
    }
}
