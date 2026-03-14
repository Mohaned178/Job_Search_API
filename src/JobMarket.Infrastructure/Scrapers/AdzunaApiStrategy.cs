using System.Text.Json;
using JobMarket.Application.Common.Interfaces;
using JobMarket.Domain.Enums;
using InfraScraperOptions = JobMarket.Infrastructure.Options.ScraperOptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobMarket.Infrastructure.Scrapers;

public class AdzunaApiStrategy : IScraperStrategy
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AdzunaApiStrategy> _logger;
    private readonly InfraScraperOptions _options;

    public JobSource Source => JobSource.Adzuna;

    public AdzunaApiStrategy(
        HttpClient httpClient,
        IOptions<InfraScraperOptions> options,
        ILogger<AdzunaApiStrategy> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<List<RawJobListing>> ScrapeAsync(
        Application.Common.Interfaces.ScraperOptions options,
        CancellationToken ct = default)
    {
        List<RawJobListing> listings = new();

        for (int page = 1; page <= options.MaxPages; page++)
        {
            string url = $"https://api.adzuna.com/v1/api/jobs/gb/search/{page}" +
                         $"?app_id={_options.AdzunaAppId}" +
                         $"&app_key={_options.AdzunaApiKey}" +
                         $"&results_per_page=20" +
                         $"&what={Uri.EscapeDataString(options.Keyword ?? "")}" +
                         $"&where={Uri.EscapeDataString(options.Location ?? "")}" +
                         "&content-type=application/json";

            _logger.LogInformation("Calling Adzuna API page {Page}", page);

            try
            {
                string json = await _httpClient.GetStringAsync(url, ct);
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement results = doc.RootElement.GetProperty("results");

                foreach (JsonElement job in results.EnumerateArray())
                {
                    string id = job.TryGetProperty("id", out JsonElement idEl) ? idEl.GetString() ?? Guid.NewGuid().ToString() : Guid.NewGuid().ToString();
                    string title = job.TryGetProperty("title", out JsonElement titleEl) ? titleEl.GetString() ?? string.Empty : string.Empty;
                    string company = job.TryGetProperty("company", out JsonElement compEl) && compEl.TryGetProperty("display_name", out JsonElement dispEl)
                        ? dispEl.GetString() ?? string.Empty : string.Empty;
                    string location = job.TryGetProperty("location", out JsonElement locEl) && locEl.TryGetProperty("display_name", out JsonElement locDispEl)
                        ? locDispEl.GetString() ?? string.Empty : string.Empty;
                    string description = job.TryGetProperty("description", out JsonElement descEl) ? descEl.GetString() ?? string.Empty : string.Empty;
                    string sourceUrl = job.TryGetProperty("redirect_url", out JsonElement urlEl) ? urlEl.GetString() ?? string.Empty : string.Empty;
                    string? salaryMin = job.TryGetProperty("salary_min", out JsonElement salMinEl) ? salMinEl.ToString() : null;

                    listings.Add(new RawJobListing(
                        title, company, "GB", location, description, sourceUrl, id,
                        DateTime.UtcNow, salaryMin));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to call Adzuna API page {Page}", page);
                break;
            }
        }

        return listings;
    }
}
