using AeroMes.Domain.Storage;
using AeroMes.Domain.Storage.Repositories;
using AeroMes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AeroMes.Infrastructure.Storage;

public sealed class FileObjectRepository(AppDbContext db) : IFileObjectRepository
{
    public Task<FileObject?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.FileObjects.FirstOrDefaultAsync(f => f.Id == id, ct);

    public async Task<IReadOnlyList<FileObject>> GetByOwnerAsync(
        string ownerType, string ownerId, CancellationToken ct)
    {
        return await db.FileObjects
            .AsNoTracking()
            .Where(f => f.OwnerType == ownerType && f.OwnerId == ownerId && !f.IsDeleted)
            .OrderByDescending(f => f.UploadedAt)
            .ToListAsync(ct);
    }

    public void Add(FileObject file) => db.FileObjects.Add(file);
}
