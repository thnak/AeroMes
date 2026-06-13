using System.Security.Cryptography;
using AeroMes.Application.Common;
using AeroMes.Application.Interfaces;
using AeroMes.Domain.Storage;
using AeroMes.Domain.Storage.Repositories;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Storage.Commands.UploadFile;

public sealed class UploadFileHandler(
    IFileStorage fileStorage,
    IThumbnailGenerator thumbGen,
    IFileObjectRepository repo,
    IUnitOfWork uow
) : ICommandHandler<UploadFileCommand, ValidationResult<FileUploadResult>>
{
    private static readonly HashSet<string> AllowedTypes =
    [
        "image/jpeg", "image/png", "image/webp", "image/gif",
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "text/plain", "text/csv",
    ];

    private const long MaxSizeBytes = 50 * 1024 * 1024; // 50 MB

    public async Task<ValidationResult<FileUploadResult>> HandleAsync(
        UploadFileCommand cmd, CancellationToken ct)
    {
        if (!AllowedTypes.Contains(cmd.ContentType.ToLowerInvariant()))
            return ValidationResult<FileUploadResult>.Failure("File type not allowed.");
        if (cmd.SizeBytes > MaxSizeBytes)
            return ValidationResult<FileUploadResult>.Failure("File exceeds 50 MB limit.");

        // Compute checksum and upload
        using var sha = SHA256.Create();
        using var ms = new MemoryStream();
        await cmd.FileStream.CopyToAsync(ms, ct);
        var hash = Convert.ToHexString(sha.ComputeHash(ms.ToArray())).ToLowerInvariant();

        var ext = Path.GetExtension(cmd.FileName);
        var key = $"{cmd.OwnerType}/{cmd.OwnerId}/{Guid.NewGuid():N}{ext}";

        ms.Position = 0;
        await fileStorage.SaveAsync(ms, key, cmd.ContentType, ct);

        // Generate thumbnail for images
        string? thumbKey = null;
        if (thumbGen.CanProcess(cmd.ContentType))
        {
            ms.Position = 0;
            var thumbStream = await thumbGen.GenerateAsync(ms, cmd.ContentType, ct: ct);
            if (thumbStream is not null)
            {
                thumbKey = $"thumbnails/{key}";
                await using (thumbStream)
                    await fileStorage.SaveAsync(thumbStream, thumbKey, "image/jpeg", ct);
            }
        }

        var fileObj = FileObject.Create(
            key, cmd.FileName, cmd.ContentType,
            cmd.SizeBytes, hash, cmd.OwnerType, cmd.OwnerId, cmd.UploadedBy);

        repo.Add(fileObj);
        await uow.SaveChangesAsync(ct);

        return ValidationResult<FileUploadResult>.Ok(new FileUploadResult(
            fileObj.Id, fileObj.FileName, fileObj.ContentType,
            fileObj.SizeBytes, fileObj.OwnerType, fileObj.OwnerId,
            thumbKey, fileObj.UploadedAt));
    }
}
