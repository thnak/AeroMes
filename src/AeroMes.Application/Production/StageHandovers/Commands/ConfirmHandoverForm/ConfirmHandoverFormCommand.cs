using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Production.StageHandovers.Commands.ConfirmHandoverForm;

public sealed record ConfirmHandoverFormCommand(int FormId, string ConfirmedBy)
    : ICommand<ValidationResult<Unit>>;
