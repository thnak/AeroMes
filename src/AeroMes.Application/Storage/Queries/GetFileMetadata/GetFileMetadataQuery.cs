using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Storage.Queries.GetFileMetadata;

public sealed record GetFileMetadataQuery(Guid Id) : IQuery<FileObjectDto?>;

public sealed record FileObjectDto(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string OwnerType,
    string OwnerId,
    string UploadedBy,
    DateTime UploadedAt,
    string DownloadUrl,
    string? ThumbnailUrl);
