namespace JobMarket.Application.Common.Interfaces;

public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default);
    Task<List<float[]>> GenerateBatchEmbeddingsAsync(List<string> texts, CancellationToken ct = default);
}
