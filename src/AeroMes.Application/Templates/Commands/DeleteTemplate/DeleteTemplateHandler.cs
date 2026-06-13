using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Templates.Commands.DeleteTemplate;

public sealed class DeleteTemplateHandler(IDocumentTemplateRepository repo)
    : ICommandHandler<DeleteTemplateCommand, ValidationResult<Unit>>
{
    public async Task<ValidationResult<Unit>> HandleAsync(
        DeleteTemplateCommand cmd, CancellationToken ct = default)
    {
        var template = await repo.GetByIdAsync(cmd.TemplateId, ct);
        if (template is null)
            return ValidationResult<Unit>.NotFound($"Mẫu in #{cmd.TemplateId} không tồn tại.");

        await repo.DeleteAsync(template, ct);
        return ValidationResult<Unit>.Ok(Unit.Value);
    }
}
