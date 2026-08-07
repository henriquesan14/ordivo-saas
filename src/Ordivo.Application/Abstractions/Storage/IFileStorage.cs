namespace Ordivo.Application.Abstractions.Storage;

public sealed record StoredFile(Stream Content, string ContentType, long Length);

public interface IFileStorage
{
    Task UploadAsync(string key, Stream content, string contentType, CancellationToken ct);
    Task<StoredFile?> DownloadAsync(string key, CancellationToken ct);
    Task DeleteAsync(string key, CancellationToken ct);
}
