namespace JobMarket.Infrastructure.Options;

public class ScraperOptions
{
    public int MaxPagesPerSource { get; set; } = 5;
    public int RequestDelayMs { get; set; } = 500;
    public string AdzunaAppId { get; set; } = string.Empty;
    public string AdzunaApiKey { get; set; } = string.Empty;
}
