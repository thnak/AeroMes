using LiteBus.Commands.Abstractions;
using AeroMes.Application.Common;

namespace AeroMes.Application.Master.ShiftTemplates.Commands.DeleteShiftTemplate;

public record DeleteShiftTemplateCommand(string Code, string? DeletedBy = null) : ICommand<ValidationResult<Unit>>;
