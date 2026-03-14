using JobMarket.Application.Common.Interfaces;
using JobMarket.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Embeddings;

namespace JobMarket.Infrastructure.AI;

public class OpenAiEmbeddingService : IEmbeddingService
{
    private readonly EmbeddingClient _client;
    private readonly ILogger<OpenAiEmbeddingService> _logger;
    private readonly string _model;

    public OpenAiEmbeddingService(
        IOptions<OpenAiOptions> options,
        ILogger<OpenAiEmbeddingService> logger)
    {
        _logger = logger;
        _model = options.Value.EmbeddingModel;
        _client = new EmbeddingClient(_model, options.Value.ApiKey);
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        _logger.LogInformation("Generating embedding for text of length {Length}", text.Length);
        var result = await _client.GenerateEmbeddingAsync(text, cancellationToken: ct);
        return result.Value.ToFloats().ToArray();
    }

    public async Task<List<float[]>> GenerateBatchEmbeddingsAsync(List<string> texts, CancellationToken ct = default)
    {
        _logger.LogInformation("Generating batch embeddings for {Count} texts", texts.Count);
        var result = await _client.GenerateEmbeddingsAsync(texts, cancellationToken: ct);
        return result.Value.Select(e => e.ToFloats().ToArray()).ToList();
    }
}
