namespace Assura.Application.Common.Interfaces;

public interface IDatabaseBackupService
{
    Task<byte[]> GenerateSqlBackupAsync();
}
