using JobMarket.Application.Common.Interfaces;
using JobMarket.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace JobMarket.Infrastructure.Scrapers;

public class ScraperFactory
{
    private readonly Dictionary<JobSource, IScraperStrategy> _strategies;
    private readonly ILogger<ScraperFactory> _logger;

    public ScraperFactory(
        IEnumerable<IScraperStrategy> strategies,
        ILogger<ScraperFactory> logger)
    {
        _logger = logger;
        _strategies = strategies.ToDictionary(s => s.Source);
    }

    public IScraperStrategy GetStrategy(JobSource source)
    {
        if (_strategies.TryGetValue(source, out IScraperStrategy? strategy))
            return strategy;

        _logger.LogWarning("No scraper strategy found for source {Source}", source);
        throw new NotSupportedException($"No scraper strategy registered for source '{source}'.");
    }

    public IEnumerable<IScraperStrategy> GetAll() => _strategies.Values;
}
