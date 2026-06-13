using AeroMes.Application.Common;
using AeroMes.Application.Wms.Commands.CreateMaterialSupplyRequest;
using AeroMes.Domain.Wms;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.UpdateMaterialSupplyRequest;

public record UpdateMaterialSupplyRequestCommand(
    int RequestId,
    MaterialSupplyRequestType RequestType,
    string RequesterUnit,
    DateTime? RequiredByDate,
    string? Notes,
    IReadOnlyList<SupplyRequestLineInput> Lines,
    string? UpdatedBy
) : ICommand<ValidationResult<Unit>>;
