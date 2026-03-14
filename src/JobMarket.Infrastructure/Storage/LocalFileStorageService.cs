using JobMarket.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace JobMarket.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        _basePath = Path.Combine(Path.GetTempPath(), "JobMarketCVs");
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        string uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        string fullPath = Path.Combine(_basePath, uniqueFileName);

        _logger.LogInformation("Uploading file {FileName} to {Path}", fileName, fullPath);

        await using FileStream fs = File.Create(fullPath);
        await fileStream.CopyToAsync(fs, ct);

        return uniqueFileName;
    }

    public Task<Stream> DownloadAsync(string storagePath, CancellationToken ct = default)
    {
        string fullPath = Path.Combine(_basePath, storagePath);
        _logger.LogInformation("Downloading file from {Path}", fullPath);
        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        string fullPath = Path.Combine(_basePath, storagePath);
        _logger.LogInformation("Deleting file {Path}", fullPath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
