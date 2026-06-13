using AeroMes.Application.Common;
using AeroMes.Domain.Templates;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Templates.Commands.UpdateTemplate;

public record UpdateTemplateCommand(
    int TemplateId,
    string TemplateName,
    PrintOutputFormat OutputFormat,
    bool IsActive,
    string? UpdatedBy) : ICommand<ValidationResult<Unit>>;
