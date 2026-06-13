using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using AeroMes.Domain.Templates;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Templates.Commands.CreateTemplate;

public sealed class CreateTemplateHandler(IDocumentTemplateRepository repo)
    : ICommandHandler<CreateTemplateCommand, ValidationResult<int>>
{
    public async Task<ValidationResult<int>> HandleAsync(
        CreateTemplateCommand cmd, CancellationToken ct = default)
    {
        try
        {
            var template = DocumentTemplate.Create(
                cmd.TemplateName, cmd.DocumentType, cmd.OutputFormat, cmd.FileId, cmd.CreatedBy);
            await repo.AddAsync(template, ct);
            return ValidationResult<int>.Ok(template.TemplateId);
        }
        catch (DomainException ex)
        {
            return ValidationResult<int>.Failure(ex.Message);
        }
    }
}
