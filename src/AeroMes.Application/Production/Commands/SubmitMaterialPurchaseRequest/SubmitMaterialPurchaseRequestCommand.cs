using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.SubmitMaterialPurchaseRequest;

public record SubmitMaterialPurchaseRequestCommand(int RequestID) : ICommand<ValidationResult<Unit>>;
