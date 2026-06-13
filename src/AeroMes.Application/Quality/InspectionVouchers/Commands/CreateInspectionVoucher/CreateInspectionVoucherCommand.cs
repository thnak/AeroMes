using AeroMes.Application.Common;
using AeroMes.Domain.Quality;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Quality.InspectionVouchers.Commands.CreateInspectionVoucher;

public record CreateInspectionVoucherCommand(
    string VoucherNumber,
    string VoucherName,
    InspectionVoucherType InspectionType,
    string InspectorName,
    DateOnly InspectionDate,
    int? LinkedRequestId,
    int? ProductionOrderId,
    decimal SampleQuantity,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
