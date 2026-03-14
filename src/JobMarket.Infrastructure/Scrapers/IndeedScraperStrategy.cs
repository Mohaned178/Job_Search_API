using HtmlAgilityPack;
using JobMarket.Application.Common.Interfaces;
using JobMarket.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace JobMarket.Infrastructure.Scrapers;

public class IndeedScraperStrategy : IScraperStrategy
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IndeedScraperStrategy> _logger;

    public JobSource Source => JobSource.Indeed;

    public IndeedScraperStrategy(
        HttpClient httpClient,
        ILogger<IndeedScraperStrategy> logger)
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
            string url = $"https://www.indeed.com/jobs?q={Uri.EscapeDataString(options.Keyword ?? "")}&l={Uri.EscapeDataString(options.Location ?? "")}&start={page * 10}";
            _logger.LogInformation("Scraping Indeed page {Page}: {Url}", page, url);

            try
            {
                string html = await _httpClient.GetStringAsync(url, ct);
                HtmlDocument doc = new();
                doc.LoadHtml(html);

                HtmlNodeCollection? cards = doc.DocumentNode.SelectNodes("//div[contains(@class,'job_seen_beacon')]");
                if (cards == null) break;

                foreach (HtmlNode card in cards)
                {
                    string title = card.SelectSingleNode(".//h2[contains(@class,'jobTitle')]//span")?.InnerText.Trim() ?? string.Empty;
                    string company = card.SelectSingleNode(".//span[@data-testid='company-name']")?.InnerText.Trim() ?? string.Empty;
                    string location = card.SelectSingleNode(".//div[@data-testid='text-location']")?.InnerText.Trim() ?? string.Empty;
                    string href = card.SelectSingleNode(".//h2//a[@href]")?.GetAttributeValue("href", string.Empty) ?? string.Empty;
                    string jobId = System.Web.HttpUtility.ParseQueryString(new Uri(href, UriKind.RelativeOrAbsolute).Query).Get("jk") ?? Guid.NewGuid().ToString();

                    if (string.IsNullOrWhiteSpace(title)) continue;

                    listings.Add(new RawJobListing(
                        title,
                        company,
                        "US",
                        location,
                        string.Empty,
                        $"https://www.indeed.com{href}",
                        jobId,
                        DateTime.UtcNow,
                        null));
                }

                await Task.Delay(500, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to scrape Indeed page {Page}", page);
                break;
            }
        }

        return listings;
    }
}
