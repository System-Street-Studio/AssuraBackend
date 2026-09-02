using Assura.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace Assura.Infrastructure.Services;

/// <summary>
/// Writes to the container/host's local wwwroot/uploads folder, exactly as this app did before
/// file storage was made pluggable. Only suitable for single-instance/local-dev deployments —
/// files written here do not survive a pod reschedule under Kubernetes.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _webRootPath;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _webRootPath = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    }

    public async Task<string> SaveAsync(Stream content, string subfolder, string fileName, string? contentType, CancellationToken cancellationToken = default)
    {
        var uploadsDir = Path.Combine(_webRootPath, "uploads", subfolder);
        Directory.CreateDirectory(uploadsDir);

        var filePath = Path.Combine(uploadsDir, fileName);
        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await content.CopyToAsync(fileStream, cancellationToken);

        return $"/uploads/{subfolder}/{fileName}".Replace("//", "/");
    }

    public Task DeleteAsync(string virtualPath, CancellationToken cancellationToken = default)
    {
        var relative = virtualPath.TrimStart('/').Replace("uploads/", string.Empty);
        var filePath = Path.Combine(_webRootPath, "uploads", relative);
        if (File.Exists(filePath)) File.Delete(filePath);
        return Task.CompletedTask;
    }

    // Files under wwwroot/uploads are already served directly by UseStaticFiles(), so callers
    // should just use the virtual path as-is — no redirect needed.
    public Task<string?> GetDownloadUrlAsync(string virtualPath, CancellationToken cancellationToken = default)
        => Task.FromResult<string?>(null);
}
