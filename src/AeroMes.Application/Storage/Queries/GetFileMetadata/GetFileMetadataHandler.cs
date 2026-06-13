using AeroMes.Domain.Storage.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Storage.Queries.GetFileMetadata;

public sealed class GetFileMetadataHandler(
    IFileObjectRepository repo,
    IFileStorage fileStorage
) : IQueryHandler<GetFileMetadataQuery, FileObjectDto?>
{
    public async Task<FileObjectDto?> HandleAsync(GetFileMetadataQuery query, CancellationToken ct)
    {
        var file = await repo.GetByIdAsync(query.Id, ct);
        if (file is null || file.IsDeleted) return null;

        var url = await fileStorage.GetUrlAsync(file.StorageKey, TimeSpan.FromHours(1), ct);
        string? thumbUrl = null;
        var thumbKey = $"thumbnails/{file.StorageKey}";
        if (await fileStorage.ExistsAsync(thumbKey, ct))
            thumbUrl = await fileStorage.GetUrlAsync(thumbKey, TimeSpan.FromHours(1), ct);

        return new FileObjectDto(
            file.Id, file.FileName, file.ContentType, file.SizeBytes,
            file.OwnerType, file.OwnerId, file.UploadedBy, file.UploadedAt,
            url, thumbUrl);
    }
}
