using AeroMes.Application.Common;
using AeroMes.Domain.Templates;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Templates.Commands.CreateTemplate;

public record CreateTemplateCommand(
    string TemplateName,
    DocumentType DocumentType,
    PrintOutputFormat OutputFormat,
    Guid FileId,
    string? CreatedBy) : ICommand<ValidationResult<int>>;
