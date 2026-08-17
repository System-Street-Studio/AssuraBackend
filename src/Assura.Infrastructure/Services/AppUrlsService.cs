using Assura.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Assura.Infrastructure.Services;

public class AppUrlsService : IAppUrlsService
{
    public string FrontendBaseUrl { get; }

    public AppUrlsService(IConfiguration configuration)
    {
        FrontendBaseUrl = (configuration["App:FrontendBaseUrl"] ?? "http://localhost:4200").TrimEnd('/');
    }
}
