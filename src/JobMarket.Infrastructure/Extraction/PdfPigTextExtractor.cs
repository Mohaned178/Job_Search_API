using JobMarket.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;

namespace JobMarket.Infrastructure.Extraction;

public class PdfPigTextExtractor : ICvTextExtractor
{
    private readonly ILogger<PdfPigTextExtractor> _logger;

    public PdfPigTextExtractor(ILogger<PdfPigTextExtractor> logger)
    {
        _logger = logger;
    }

    public Task<string> ExtractAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        _logger.LogInformation("Extracting text from PDF {FileName}", fileName);

        using PdfDocument pdf = PdfDocument.Open(fileStream);
        string text = string.Join(Environment.NewLine,
            pdf.GetPages().Select(p => p.Text));

        return Task.FromResult(text);
    }
}
