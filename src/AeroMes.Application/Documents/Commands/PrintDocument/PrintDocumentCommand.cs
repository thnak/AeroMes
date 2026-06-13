using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Documents.Commands.PrintDocument;

public record PrintDocumentCommand(
    string DocumentType,
    string DocumentId,
    int? TemplateId,
    string PrintedBy) : ICommand<ValidationResult<PrintDocumentResult>>;
