using AeroMes.Application.Storage.Queries.GetFileMetadata;
using LiteBus.Queries.Abstractions;

namespace AeroMes.Application.Storage.Queries.ListFiles;

public sealed record ListFilesQuery(string OwnerType, string OwnerId)
    : IQuery<IReadOnlyList<FileObjectDto>>;
