using JobMarket.Application.Common.Interfaces;
using JobMarket.Infrastructure.Extraction;
using JobMarket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobMarket.Infrastructure.Jobs;

public class CvProcessingJob : ICvProcessingJob
{
    private readonly AppDbContext _context;
    private readonly CvTextExtractorFactory _extractorFactory;
    private readonly ICvParsingService _parsingService;
    private readonly IEmbeddingService _embeddingService;
    private readonly IAtsService _atsService;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<CvProcessingJob> _logger;

    public CvProcessingJob(
        AppDbContext context,
        CvTextExtractorFactory extractorFactory,
        ICvParsingService parsingService,
        IEmbeddingService embeddingService,
        IAtsService atsService,
        IFileStorageService fileStorage,
        ILogger<CvProcessingJob> logger)
    {
        _context = context;
        _extractorFactory = extractorFactory;
        _parsingService = parsingService;
        _embeddingService = embeddingService;
        _atsService = atsService;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task ProcessAsync(Guid cvId, CancellationToken ct)
    {
        _logger.LogInformation("Processing CV {CvId}", cvId);

        var cv = await _context.CVs.FindAsync(new object[] { cvId }, ct);
        if (cv == null)
        {
            _logger.LogWarning("CV {CvId} not found", cvId);
            return;
        }

        try
        {
            cv.MarkProcessing();
            await _context.SaveChangesAsync(ct);

            using Stream fileStream = await _fileStorage.DownloadAsync(cv.StoragePath, ct);
            var extractor = _extractorFactory.GetExtractor(cv.OriginalFileName);
            string rawText = await extractor.ExtractAsync(fileStream, cv.OriginalFileName, ct);

            var parsedData = await _parsingService.ParseAsync(rawText, ct);
            string embeddingText = $"{parsedData.Summary} {string.Join(" ", parsedData.Skills)}";
            float[] embedding = await _embeddingService.GenerateEmbeddingAsync(embeddingText, ct);

            (int score, var breakdown) = await _atsService.ScoreAsync(parsedData, ct);

            cv.MarkEnriched(parsedData, embedding, score, breakdown);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("CV {CvId} processed successfully with ATS score {Score}", cvId, score);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process CV {CvId}", cvId);
            cv.MarkFailed();
            await _context.SaveChangesAsync(ct);
        }
    }
}
