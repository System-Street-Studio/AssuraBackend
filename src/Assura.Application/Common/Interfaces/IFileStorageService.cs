namespace Assura.Application.Common.Interfaces;

/// <summary>
/// Abstracts where uploaded files (receipts, product images, asset-request attachments) live.
/// Callers work only in terms of a stable "virtual path" of the form "/uploads/{subfolder}/{fileName}"
/// so the DB schema and API responses are identical regardless of which implementation is active.
/// </summary>
public interface IFileStorageService
{
    /// <summary>Saves the stream and returns its virtual path, e.g. "/uploads/receipts/16_20260823.png".</summary>
    Task<string> SaveAsync(Stream content, string subfolder, string fileName, string? contentType, CancellationToken cancellationToken = default);

    Task DeleteAsync(string virtualPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a URL the caller can redirect a client to in order to download the file, or null
    /// if the file is already reachable at its virtual path directly (e.g. served as a static file).
    /// </summary>
    Task<string?> GetDownloadUrlAsync(string virtualPath, CancellationToken cancellationToken = default);
}
