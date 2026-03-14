using JobMarket.Application.Common.Interfaces;

namespace JobMarket.Infrastructure.Extraction;

public class CvTextExtractorFactory
{
    private readonly PdfPigTextExtractor _pdfExtractor;
    private readonly DocxTextExtractor _docxExtractor;

    public CvTextExtractorFactory(
        PdfPigTextExtractor pdfExtractor,
        DocxTextExtractor docxExtractor)
    {
        _pdfExtractor = pdfExtractor;
        _docxExtractor = docxExtractor;
    }

    public ICvTextExtractor GetExtractor(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => _pdfExtractor,
            ".docx" => _docxExtractor,
            _ => throw new NotSupportedException($"File extension '{extension}' is not supported.")
        };
    }
}
