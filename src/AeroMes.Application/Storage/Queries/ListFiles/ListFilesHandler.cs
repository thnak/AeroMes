using AeroMes.Application.Storage.Queries.GetFileMetadata;
using AeroMes.Domain.Storage.Repositories;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Storage.Queries.ListFiles;

public sealed class ListFilesHandler(
    IFileObjectRepository repo,
    IFileStorage fileStorage
) : IQueryHandler<ListFilesQuery, IReadOnlyList<FileObjectDto>>
{
    public async Task<IReadOnlyList<FileObjectDto>> HandleAsync(ListFilesQuery query, CancellationToken ct)
    {
        var files = await repo.GetByOwnerAsync(query.OwnerType, query.OwnerId, ct);
        var result = new List<FileObjectDto>(files.Count);

        foreach (var f in files.Where(f => !f.IsDeleted))
        {
            var url = await fileStorage.GetUrlAsync(f.StorageKey, TimeSpan.FromHours(1), ct);
            string? thumbUrl = null;
            var thumbKey = $"thumbnails/{f.StorageKey}";
            if (await fileStorage.ExistsAsync(thumbKey, ct))
                thumbUrl = await fileStorage.GetUrlAsync(thumbKey, TimeSpan.FromHours(1), ct);

            result.Add(new FileObjectDto(
                f.Id, f.FileName, f.ContentType, f.SizeBytes,
                f.OwnerType, f.OwnerId, f.UploadedBy, f.UploadedAt,
                url, thumbUrl));
        }

        return result;
    }
}
