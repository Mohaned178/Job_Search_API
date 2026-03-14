using System.Text.Json;
using JobMarket.Application.Common.Interfaces;
using JobMarket.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace JobMarket.Infrastructure.Scrapers;

public class RemotiveApiStrategy : IScraperStrategy
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RemotiveApiStrategy> _logger;

    public JobSource Source => JobSource.Remotive;

    public RemotiveApiStrategy(
        HttpClient httpClient,
        ILogger<RemotiveApiStrategy> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<RawJobListing>> ScrapeAsync(
        Application.Common.Interfaces.ScraperOptions options,
        CancellationToken ct = default)
    {
        List<RawJobListing> listings = new();

        string url = $"https://remotive.com/api/remote-jobs?search={Uri.EscapeDataString(options.Keyword ?? "")}&limit=100";
        _logger.LogInformation("Calling Remotive API");

        try
        {
            string json = await _httpClient.GetStringAsync(url, ct);
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement jobs = doc.RootElement.GetProperty("jobs");

            int count = 0;
            foreach (JsonElement job in jobs.EnumerateArray())
            {
                if (count++ >= options.MaxPages * 20) break;

                string id = job.TryGetProperty("id", out JsonElement idEl) ? idEl.GetRawText() : Guid.NewGuid().ToString();
                string title = job.TryGetProperty("title", out JsonElement titleEl) ? titleEl.GetString() ?? string.Empty : string.Empty;
                string company = job.TryGetProperty("company_name", out JsonElement compEl) ? compEl.GetString() ?? string.Empty : string.Empty;
                string description = job.TryGetProperty("description", out JsonElement descEl) ? descEl.GetString() ?? string.Empty : string.Empty;
                string sourceUrl = job.TryGetProperty("url", out JsonElement urlEl) ? urlEl.GetString() ?? string.Empty : string.Empty;
                string postedAtStr = job.TryGetProperty("publication_date", out JsonElement dateEl) ? dateEl.GetString() ?? string.Empty : string.Empty;

                DateTime.TryParse(postedAtStr, out DateTime postedAt);

                listings.Add(new RawJobListing(
                    title, company, "Remote", "Remote", description, sourceUrl, id,
                    postedAt == default ? DateTime.UtcNow : postedAt, null));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to call Remotive API");
        }

        return listings;
    }
}
