using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.Commands.ApproveMaterialPurchaseRequest;

public record ApproveMaterialPurchaseRequestCommand(int RequestID, bool IsApproved) : ICommand<ValidationResult<Unit>>;
