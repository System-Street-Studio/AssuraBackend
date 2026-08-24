using Assura.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assura.API.Controllers;

// Serves uploaded files at the same "/uploads/..." URLs the app has always used, so DB-stored
// FileUrl/ImageUrl values and the frontend need no changes regardless of storage backend.
// In Local mode, UseStaticFiles() (registered earlier in the pipeline) already serves these
// paths directly from disk, so this controller is never reached. In S3 mode, no physical file
// exists locally, so the request falls through to here and gets redirected to a short-lived
// pre-signed S3 URL. Anonymous access matches today's behavior: UseStaticFiles() serves
// "/uploads/..." with no authorization check either.
[AllowAnonymous]
[ApiController]
[Route("uploads")]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _fileStorage;

    public FilesController(IFileStorageService fileStorage)
    {
        _fileStorage = fileStorage;
    }

    [HttpGet("{**path}")]
    public async Task<IActionResult> Get(string path)
    {
        var virtualPath = $"/uploads/{path}";
        var downloadUrl = await _fileStorage.GetDownloadUrlAsync(virtualPath);
        if (downloadUrl == null) return NotFound();
        return Redirect(downloadUrl);
    }
}
