using System.IO.Packaging;
using System.Xml.Linq;
using JobMarket.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace JobMarket.Infrastructure.Extraction;

public class DocxTextExtractor : ICvTextExtractor
{
    private readonly ILogger<DocxTextExtractor> _logger;

    public DocxTextExtractor(ILogger<DocxTextExtractor> logger)
    {
        _logger = logger;
    }

    public Task<string> ExtractAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        _logger.LogInformation("Extracting text from DOCX {FileName}", fileName);

        using Package package = Package.Open(fileStream);
        PackagePart? documentPart = package.GetPart(new Uri("/word/document.xml", UriKind.Relative));

        if (documentPart == null)
            return Task.FromResult(string.Empty);

        using Stream docStream = documentPart.GetStream();
        XDocument doc = XDocument.Load(docStream);

        XNamespace wNs = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        string text = string.Join(
            " ",
            doc.Descendants(wNs + "t").Select(t => t.Value));

        return Task.FromResult(text);
    }
}
