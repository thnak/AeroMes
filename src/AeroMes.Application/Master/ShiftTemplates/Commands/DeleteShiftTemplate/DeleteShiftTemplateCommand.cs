using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Master.ShiftTemplates.Commands.DeleteShiftTemplate;

public record DeleteShiftTemplateCommand(string Code, string? DeletedBy = null) : ICommand;
