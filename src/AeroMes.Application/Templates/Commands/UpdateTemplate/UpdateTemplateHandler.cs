using AeroMes.Application.Common;
using AeroMes.Domain.Exceptions;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Templates.Commands.UpdateTemplate;

public sealed class UpdateTemplateHandler(IDocumentTemplateRepository repo)
    : ICommandHandler<UpdateTemplateCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        UpdateTemplateCommand cmd, CancellationToken ct = default)
    {
        var template = await repo.GetByIdAsync(cmd.TemplateId, ct);
        if (template is null)
            return ValidationResult<Unit>.NotFound($"Mẫu in #{cmd.TemplateId} không tồn tại.");

        try
        {
            template.Update(cmd.TemplateName, cmd.OutputFormat, cmd.IsActive, cmd.UpdatedBy);
            await repo.SaveChangesAsync(ct);
            return ValidationResult<Unit>.Ok(Unit.Value);
        }
        catch (DomainException ex)
        {
            return ValidationResult<Unit>.Failure(ex.Message);
        }
    }
}
