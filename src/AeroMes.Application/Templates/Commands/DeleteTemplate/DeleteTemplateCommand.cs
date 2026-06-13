using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Templates.Commands.DeleteTemplate;

public record DeleteTemplateCommand(int TemplateId) : ICommand<ValidationResult<Unit>>;
