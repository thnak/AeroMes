using AeroMes.Application.Common;
using AeroMes.Domain.Quality;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionVouchers.Commands.UpdateVoucherStatus;

public record UpdateVoucherStatusCommand(
    int VoucherID,
    string Action,
    InspectionConclusion? Conclusion,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
