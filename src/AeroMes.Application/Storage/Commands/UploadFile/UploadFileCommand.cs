using AeroMes.Application.Common;
using LiteBus.Commands.Abstractions;

namespace AeroMes.Application.Storage.Commands.UploadFile;

public sealed record UploadFileCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    long SizeBytes,
    string OwnerType,
    string OwnerId,
    string UploadedBy
) : ICommand<ValidationResult<FileUploadResult>>;

public sealed record FileUploadResult(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string OwnerType,
    string OwnerId,
    string? ThumbnailKey,
    DateTime UploadedAt);
