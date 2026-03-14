using System.Text.Json;
using JobMarket.Application.Common.Interfaces;
using JobMarket.Application.Common.Models;
using JobMarket.Domain.Enums;
using JobMarket.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace JobMarket.Infrastructure.AI;

public class OpenAiJobEnrichmentService : IJobEnrichmentService
{
    private readonly ChatClient _client;
    private readonly ILogger<OpenAiJobEnrichmentService> _logger;

    public OpenAiJobEnrichmentService(
        IOptions<OpenAiOptions> options,
        ILogger<OpenAiJobEnrichmentService> logger)
    {
        _logger = logger;
        _client = new ChatClient(options.Value.ChatModel, options.Value.ApiKey);
    }

    public async Task<JobEnrichmentResult> EnrichAsync(
        string title,
        string description,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Enriching job {Title}", title);

        string prompt = $$"""
            Analyze this job posting and extract structured data. Return ONLY valid JSON:
            {
              "Skills": ["string"],
              "SeniorityLevel": "Intern|Junior|Mid|Senior|Lead|Principal",
              "SalaryMin": null or number,
              "SalaryMax": null or number,
              "Currency": "USD or empty string"
            }

            Job Title: {{title}}
            Job Description: {{description[..Math.Min(description.Length, 2000)]}}
            """;

        var result = await _client.CompleteChatAsync(
            new[] { new UserChatMessage(prompt) },
            cancellationToken: ct);

        string json = result.Value.Content[0].Text;
        EnrichmentPayload? payload = JsonSerializer.Deserialize<EnrichmentPayload>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return new JobEnrichmentResult
        {
            Skills = payload?.Skills ?? new List<string>(),
            SeniorityLevel = Enum.TryParse<SeniorityLevel>(payload?.SeniorityLevel, out SeniorityLevel level)
                ? level
                : SeniorityLevel.Mid,
            SalaryMin = payload?.SalaryMin,
            SalaryMax = payload?.SalaryMax,
            Currency = payload?.Currency ?? string.Empty
        };
    }

    private sealed class EnrichmentPayload
    {
        public List<string> Skills { get; set; } = new();
        public string SeniorityLevel { get; set; } = string.Empty;
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
}
