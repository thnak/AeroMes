namespace AeroMes.Domain.Storage.Repositories;

public interface IFileObjectRepository
{
    Task<FileObject?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<FileObject>> GetByOwnerAsync(string ownerType, string ownerId, CancellationToken ct = default);
    void Add(FileObject file);
}
