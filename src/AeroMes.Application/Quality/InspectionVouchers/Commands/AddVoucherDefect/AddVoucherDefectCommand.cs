using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionVouchers.Commands.AddVoucherDefect;

public record AddVoucherDefectCommand(
    int VoucherID,
    int DefectCodeId,
    string DefectName,
    decimal Quantity,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
