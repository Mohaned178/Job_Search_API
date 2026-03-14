using System.Text.Json;
using JobMarket.Application.Common.Interfaces;
using JobMarket.Domain.ValueObjects;
using JobMarket.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace JobMarket.Infrastructure.AI;

public class OpenAiCvParsingService : ICvParsingService
{
    private readonly ChatClient _client;
    private readonly ILogger<OpenAiCvParsingService> _logger;

    public OpenAiCvParsingService(
        IOptions<OpenAiOptions> options,
        ILogger<OpenAiCvParsingService> logger)
    {
        _logger = logger;
        _client = new ChatClient(options.Value.ChatModel, options.Value.ApiKey);
    }

    public async Task<CvParsedData> ParseAsync(string rawText, CancellationToken ct = default)
    {
        _logger.LogInformation("Parsing CV text of length {Length}", rawText.Length);

        string prompt = $$"""
            Extract structured information from the following CV text. Return ONLY valid JSON matching this exact schema:
            {
              "FullName": "string",
              "Email": "string",
              "Phone": "string",
              "Summary": "string",
              "Skills": ["string"],
              "Experience": [
                {
                  "Title": "string",
                  "Company": "string",
                  "StartDate": "YYYY-MM-DD or null",
                  "EndDate": "YYYY-MM-DD or null",
                  "Description": "string"
                }
              ],
              "Education": [
                {
                  "Degree": "string",
                  "Institution": "string",
                  "StartDate": "YYYY-MM-DD or null",
                  "EndDate": "YYYY-MM-DD or null",
                  "Field": "string"
                }
              ],
              "Languages": ["string"],
              "TotalYearsExperience": 0
            }
            
            CV Text:
            {{rawText}}
            """;

        var result = await _client.CompleteChatAsync(
            new[] { new UserChatMessage(prompt) },
            cancellationToken: ct);

        string json = result.Value.Content[0].Text;
        return JsonSerializer.Deserialize<CvParsedData>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new CvParsedData();
    }
}
