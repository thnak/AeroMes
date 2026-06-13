using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Wms.Commands.ConfirmGrn;

public record ConfirmGrnCommand(int GrnId, string ConfirmedBy) : ICommand<ValidationResult<Unit>>;
