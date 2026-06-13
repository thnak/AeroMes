using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.FulfillMaterialRequisition;

public record FulfillMaterialRequisitionCommand(
    int RequisitionId,
    IReadOnlyList<IssuanceLineInput> IssuanceLines,
    string? FulfilledBy
) : ICommand<ValidationResult<Unit>>;

public record IssuanceLineInput(int LineId, decimal ActualIssuedQuantity);
