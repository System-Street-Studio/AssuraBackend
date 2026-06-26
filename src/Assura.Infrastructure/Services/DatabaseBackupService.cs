using Assura.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.IO;
using System.Threading.Tasks;

namespace Assura.Infrastructure.Services;

public class DatabaseBackupService : IDatabaseBackupService
{
    private readonly IConfiguration _configuration;

    public DatabaseBackupService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<byte[]> GenerateSqlBackupAsync()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        using (var conn = new MySqlConnection(connectionString))
        {
            using (var cmd = new MySqlCommand())
            {
                using (var mb = new MySqlBackup(cmd))
                {
                    cmd.Connection = conn;
                    await conn.OpenAsync();
                    
                    using (var ms = new MemoryStream())
                    {
                        mb.ExportToMemoryStream(ms);
                        return ms.ToArray();
                    }
                }
            }
        }
    }
}
