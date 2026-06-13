namespace AeroMes.Application.Storage;

/// <summary>
/// A request to store a file. <paramref name="Container"/> is a logical bucket/folder
/// (see <see cref="StorageContainers"/>) so SOP documents, production specs, product
/// images, etc. are kept separate and independently lifecycle-managed.
/// </summary>
public sealed record FileSaveRequest(
    Stream Content,
    string FileName,
    string ContentType,
    string Container);

/// <summary>Metadata returned after a file is stored.</summary>
public sealed record StoredFileInfo(
    string StorageKey,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Checksum);

/// <summary>An open, readable file. The caller owns and disposes <see cref="Content"/>.</summary>
public sealed record FileDownload(
    Stream Content,
    string ContentType,
    long SizeBytes,
    string FileName);
