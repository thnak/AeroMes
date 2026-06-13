using System.Text;
using AeroMes.Application.Storage;
using AeroMes.Infrastructure.Storage;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AeroMes.UnitTests.Storage;

/// <summary>
/// Pure unit tests for the default local-disk file storage provider — no Docker, no web host.
/// </summary>
public sealed class LocalDiskStorageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"aeromes-storage-test-{Guid.NewGuid():N}");

    private LocalDiskStorage CreateStorage(FileStorageOptions? options = null)
    {
        options ??= new FileStorageOptions();
        options.Local.RootPath = _root;
        return new LocalDiskStorage(
            Options.Create(options),
            new TestHostEnvironment(_root),
            NullLogger<LocalDiskStorage>.Instance);
    }

    private static FileSaveRequest Request(string content, string fileName = "spec.pdf",
        string contentType = "application/pdf", string container = StorageContainers.ProductionSpec)
        => new(new MemoryStream(Encoding.UTF8.GetBytes(content)), fileName, contentType, container);

    [Fact]
    public async Task SaveAsync_StoresFile_AndReturnsMetadata()
    {
        var storage = CreateStorage();

        var info = await storage.SaveAsync(Request("hello sop", fileName: "WI-001.pdf", container: StorageContainers.Sop));

        Assert.StartsWith($"{StorageContainers.Sop}/", info.StorageKey);
        Assert.Equal("WI-001.pdf", info.FileName);
        Assert.Equal(9, info.SizeBytes);
        Assert.Equal(64, info.Checksum.Length); // SHA-256 hex
        Assert.True(await storage.ExistsAsync(info.StorageKey));
    }

    [Fact]
    public async Task SaveThenOpenRead_RoundTripsContent()
    {
        var storage = CreateStorage();
        var info = await storage.SaveAsync(Request("round-trip body"));

        var download = await storage.OpenReadAsync(info.StorageKey);

        Assert.NotNull(download);
        using var reader = new StreamReader(download!.Content);
        Assert.Equal("round-trip body", await reader.ReadToEndAsync());
    }

    [Fact]
    public async Task Checksum_IsDeterministic_ForSameContent()
    {
        var storage = CreateStorage();

        var a = await storage.SaveAsync(Request("identical"));
        var b = await storage.SaveAsync(Request("identical"));

        Assert.Equal(a.Checksum, b.Checksum);
        Assert.NotEqual(a.StorageKey, b.StorageKey); // unique keys regardless of content
    }

    [Fact]
    public async Task DeleteAsync_RemovesFile_AndIsFalseWhenMissing()
    {
        var storage = CreateStorage();
        var info = await storage.SaveAsync(Request("to delete"));

        Assert.True(await storage.DeleteAsync(info.StorageKey));
        Assert.False(await storage.ExistsAsync(info.StorageKey));
        Assert.False(await storage.DeleteAsync(info.StorageKey));
    }

    [Fact]
    public async Task SaveAsync_RejectsOversizedFile()
    {
        var storage = CreateStorage(new FileStorageOptions { MaxSizeBytes = 4 });

        await Assert.ThrowsAsync<ArgumentException>(() => storage.SaveAsync(Request("way too big")));
    }

    [Fact]
    public async Task SaveAsync_RejectsDisallowedContentType()
    {
        var storage = CreateStorage(new FileStorageOptions { AllowedContentTypes = ["application/pdf"] });

        await Assert.ThrowsAsync<ArgumentException>(() =>
            storage.SaveAsync(Request("x", fileName: "evil.exe", contentType: "application/x-msdownload")));
    }

    [Fact]
    public async Task OpenReadAsync_ReturnsNull_ForUnknownKey()
    {
        var storage = CreateStorage();
        Assert.Null(await storage.OpenReadAsync($"{StorageContainers.General}/does-not-exist.bin"));
    }

    [Fact]
    public async Task ResolvePath_RejectsTraversalKeys()
    {
        var storage = CreateStorage();
        await Assert.ThrowsAsync<ArgumentException>(() => storage.ExistsAsync("../../etc/passwd"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }

    private sealed class TestHostEnvironment(string contentRoot) : IHostEnvironment
    {
        public string ApplicationName { get; set; } = "AeroMes.Tests";
        public string EnvironmentName { get; set; } = "Test";
        public string ContentRootPath { get; set; } = contentRoot;
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
