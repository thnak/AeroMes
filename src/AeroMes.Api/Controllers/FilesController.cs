using AeroMes.Api.Auth;
using AeroMes.Api.Extensions;
using AeroMes.Application.Storage;
using AeroMes.Application.Storage.Commands.DeleteFile;
using AeroMes.Application.Storage.Commands.UploadFile;
using AeroMes.Application.Storage.Queries.GetFileMetadata;
using AeroMes.Application.Storage.Queries.ListFiles;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroMes.Api.Controllers;

[ApiController]
[Route("api/v1/files")]
[Authorize]
public sealed class FilesController(
    ICommandMediator commandMediator,
    IQueryMediator queryMediator,
    IFileStorage fileStorage
) : ControllerBase
{
    [HttpPost]
    [RequirePermission(Permissions.FileUpload)]
    [ProducesResponseType<FileUploadResult>(StatusCodes.Status201Created)]
    [RequestSizeLimit(52_428_800)] // 50 MB
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] string ownerType,
        [FromForm] string ownerId,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file provided.");

        var userName = User.Identity?.Name ?? "system";
        await using var stream = file.OpenReadStream();

        var result = await commandMediator.SendAsync(
            new UploadFileCommand(stream, file.FileName, file.ContentType,
                file.Length, ownerType, ownerId, userName),
            null, ct);

        if (!result.IsSuccess) return result.ToErrorResult();
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<FileObjectDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var dto = await queryMediator.QueryAsync(new GetFileMetadataQuery(id), null, ct);
        if (dto is null) return NotFound();
        return Ok(dto);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<FileObjectDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] string ownerType,
        [FromQuery] string ownerId,
        CancellationToken ct)
    {
        var result = await queryMediator.QueryAsync(new ListFilesQuery(ownerType, ownerId), null, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.FileDelete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await commandMediator.SendAsync(
            new DeleteFileCommand(id, User.Identity?.Name ?? "system"), null, ct);
        if (!result.IsSuccess) return result.ToErrorResult();
        return NoContent();
    }

    // Serve files for local storage
    [HttpGet("serve/{*key}")]
    [AllowAnonymous]
    public async Task<IActionResult> Serve(string key, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key)) return BadRequest();
        try
        {
            var stream = await fileStorage.OpenReadAsync(key, ct);
            var ext = Path.GetExtension(key);
            var contentType = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
            return File(stream, contentType);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }
}
