using Assura.Application.Common.Interfaces;

using Microsoft.AspNetCore.Http;

using System.Security.Claims;



namespace Assura.Infrastructure.Services;



public class CurrentUserService : ICurrentUserService

{

    private readonly IHttpContextAccessor _httpContextAccessor;



    public CurrentUserService(IHttpContextAccessor httpContextAccessor)

    {

        _httpContextAccessor = httpContextAccessor;

    }



    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

}

