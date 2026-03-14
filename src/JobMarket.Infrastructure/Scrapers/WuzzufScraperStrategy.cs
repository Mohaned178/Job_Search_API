using HtmlAgilityPack;
using JobMarket.Application.Common.Interfaces;
using JobMarket.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace JobMarket.Infrastructure.Scrapers;

public class WuzzufScraperStrategy : IScraperStrategy
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WuzzufScraperStrategy> _logger;

    public JobSource Source => JobSource.Wuzzuf;

    public WuzzufScraperStrategy(
        HttpClient httpClient,
        ILogger<WuzzufScraperStrategy> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<RawJobListing>> ScrapeAsync(
        Application.Common.Interfaces.ScraperOptions options,
        CancellationToken ct = default)
    {
        List<RawJobListing> listings = new();

        for (int page = 0; page < options.MaxPages; page++)
        {
            string url = $"https://wuzzuf.net/search/jobs/?q={Uri.EscapeDataString(options.Keyword ?? "")}&l={Uri.EscapeDataString(options.Location ?? "")}&start={page}";
            _logger.LogInformation("Scraping Wuzzuf page {Page}: {Url}", page, url);

            try
            {
                string html = await _httpClient.GetStringAsync(url, ct);
                HtmlDocument doc = new();
                doc.LoadHtml(html);

                HtmlNodeCollection? cards = doc.DocumentNode.SelectNodes("//div[contains(@class,'css-')]//h2[contains(@class,'css-')]/..");
                if (cards == null) break;

                foreach (HtmlNode card in cards)
                {
                    string title = card.SelectSingleNode(".//h2")?.InnerText.Trim() ?? string.Empty;
                    string company = card.SelectSingleNode(".//a[contains(@class,'css-')]")?.InnerText.Trim() ?? string.Empty;
                    string location = card.SelectSingleNode(".//span[contains(@class,'css-')]")?.InnerText.Trim() ?? string.Empty;
                    string href = card.SelectSingleNode(".//a[@href]")?.GetAttributeValue("href", string.Empty) ?? string.Empty;
                    string jobId = href.Split('/').LastOrDefault() ?? Guid.NewGuid().ToString();

                    if (string.IsNullOrWhiteSpace(title)) continue;

                    listings.Add(new RawJobListing(
                        title,
                        company,
                        "Egypt",
                        location,
                        string.Empty,
                        $"https://wuzzuf.net{href}",
                        jobId,
                        DateTime.UtcNow,
                        null));
                }

                await Task.Delay(500, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to scrape Wuzzuf page {Page}", page);
                break;
            }
        }

        return listings;
    }
}
