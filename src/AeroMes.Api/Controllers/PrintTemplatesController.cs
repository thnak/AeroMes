using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Documents.Commands.PrintDocument;
using AeroMes.Application.Documents.Queries.GetDocumentPrintTemplates;
using AeroMes.Application.Documents.Queries.GetPrintAudit;
using AeroMes.Application.Templates;
using AeroMes.Application.Templates.Commands.CreateTemplate;
using AeroMes.Application.Templates.Commands.DeleteTemplate;
using AeroMes.Application.Templates.Commands.UpdateTemplate;
using AeroMes.Application.Templates.Queries.GetFieldMapping;
using AeroMes.Application.Templates.Queries.GetTemplateDetail;
using AeroMes.Application.Templates.Queries.GetTemplateList;
using AeroMes.Domain.Templates;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/print-templates")]
[Authorize]
public sealed class PrintTemplatesController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<DocumentTemplateDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] string? documentType,
        [FromQuery] bool? isActive,
        CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetTemplateListQuery(documentType, isActive), null, ct));

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<DocumentTemplateDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new GetTemplateDetailQuery(id), null, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("field-mapping")]
    [RequirePermission(Permissions.MasterDataRead)]
    [ProducesResponseType<IReadOnlyList<TemplateFieldItem>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFieldMapping(
        [FromQuery] string documentType, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetFieldMappingQuery(documentType), null, ct));

    [HttpPost]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType<int>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTemplateRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new CreateTemplateCommand(
                request.TemplateName, request.DocumentType, request.OutputFormat,
                request.FileId, User.Identity?.Name), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateTemplateRequest request, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new UpdateTemplateCommand(id, request.TemplateName, request.OutputFormat,
                request.IsActive, User.Identity?.Name), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return Ok();
    }

    [HttpDelete("{id:int}")]
    [RequirePermission(Permissions.MasterDataWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(new DeleteTemplateCommand(id), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }
}

[ApiController]
[Route("api/v1/documents/{documentType}/{documentId}")]
[Authorize]
public sealed class DocumentPrintController(ICommandMediator commandMediator, IQueryMediator queryMediator)
    : ControllerBase
{
    [HttpGet("print-templates")]
    [ProducesResponseType<IReadOnlyList<AvailableTemplateItem>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableTemplates(
        string documentType, string documentId, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetDocumentPrintTemplatesQuery(documentType), null, ct));

    [HttpPost("print")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Print(
        string documentType, string documentId,
        [FromQuery] int? templateId,
        CancellationToken ct = default)
    {
        var result = await commandMediator.SendAsync(
            new PrintDocumentCommand(documentType, documentId, templateId,
                User.Identity?.Name ?? "system"), null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        var output = result.Value!;
        return File(output.Content, output.ContentType, output.FileName);
    }

    [HttpGet("print/audit")]
    [ProducesResponseType<IReadOnlyList<PrintAuditLogDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPrintAudit(
        string documentType, string documentId, CancellationToken ct = default)
        => Ok(await queryMediator.QueryAsync(new GetPrintAuditQuery(documentType, documentId), null, ct));
}

public record CreateTemplateRequest(
    string TemplateName,
    DocumentType DocumentType,
    PrintOutputFormat OutputFormat,
    Guid FileId);

public record UpdateTemplateRequest(
    string TemplateName,
    PrintOutputFormat OutputFormat,
    bool IsActive);
