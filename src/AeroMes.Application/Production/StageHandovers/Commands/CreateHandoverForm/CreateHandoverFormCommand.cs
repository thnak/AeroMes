using AeroMes.Application.Common;
using AeroMes.Domain.Production;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.StageHandovers.Commands.CreateHandoverForm;

public sealed record HandoverLineSpec(string ProductCode, decimal Qty, string Unit, string? Notes);

public sealed record CreateHandoverFormCommand(
    HandoverFormType FormType,
    int FromWorkOrderId,
    int FromRoutingStepId,
    int ToWorkOrderId,
    int ToRoutingStepId,
    DateTime HandoverDate,
    string? Notes,
    IReadOnlyList<HandoverLineSpec> Lines,
    string CreatedBy) : ICommand<ValidationResult<int>>;
