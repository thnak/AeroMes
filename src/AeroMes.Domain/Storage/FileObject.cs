namespace AeroMes.Domain.Storage;

public sealed class FileObject
{
    public Guid Id { get; private set; }
    public string StorageKey { get; private set; } = "";
    public string FileName { get; private set; } = "";
    public string ContentType { get; private set; } = "";
    public long SizeBytes { get; private set; }
    public string Checksum { get; private set; } = "";
    public string OwnerType { get; private set; } = "";
    public string OwnerId { get; private set; } = "";
    public string UploadedBy { get; private set; } = "";
    public DateTime UploadedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private FileObject() { }

    public static FileObject Create(
        string storageKey,
        string fileName,
        string contentType,
        long sizeBytes,
        string checksum,
        string ownerType,
        string ownerId,
        string uploadedBy)
    {
        return new FileObject
        {
            Id = Guid.NewGuid(),
            StorageKey = storageKey,
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            Checksum = checksum,
            OwnerType = ownerType,
            OwnerId = ownerId,
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow,
            IsDeleted = false,
        };
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}
