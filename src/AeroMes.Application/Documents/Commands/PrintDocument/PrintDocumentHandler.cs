using AeroMes.Application.Common;
using AeroMes.Application.Templates;
using AeroMes.Domain.Templates;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Documents.Commands.PrintDocument;

public sealed class PrintDocumentHandler(
    IDocumentPrintService printService,
    IDocumentTemplateRepository templateRepo)
    : ICommandHandler<PrintDocumentCommand, ValidationResult<PrintDocumentResult>>
{
    public async Task<ValidationResult<PrintDocumentResult>> HandleAsync(
        PrintDocumentCommand cmd, CancellationToken ct = default)
    {
        PrintDocumentResult output;

        if (cmd.TemplateId.HasValue)
        {
            var template = await templateRepo.GetByIdAsync(cmd.TemplateId.Value, ct);
            if (template is null)
                return ValidationResult<PrintDocumentResult>.NotFound(
                    $"Mẫu in #{cmd.TemplateId} không tồn tại.");

            output = await printService.RenderWithTemplateAsync(
                cmd.DocumentType, cmd.DocumentId, cmd.TemplateId.Value, ct);
        }
        else
        {
            output = await printService.RenderDefaultAsync(cmd.DocumentType, cmd.DocumentId, ct);
        }

        var log = PrintAuditLog.Create(
            cmd.DocumentType, cmd.DocumentId,
            cmd.TemplateId, output.FileName,
            output.ContentType, cmd.PrintedBy);
        await templateRepo.AddPrintAuditAsync(log, ct);

        return ValidationResult<PrintDocumentResult>.Ok(output);
    }
}
