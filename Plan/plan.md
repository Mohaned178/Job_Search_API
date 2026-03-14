# Job Market Intelligence API — Technical Implementation Plan

## Solution Structure

```
JobMarketIntelligence/
├── src/
│   ├── JobMarket.Domain/
│   ├── JobMarket.Application/
│   ├── JobMarket.Infrastructure/
│   └── JobMarket.API/
├── tests/
│   ├── JobMarket.Domain.Tests/
│   ├── JobMarket.Application.Tests/
│   └── JobMarket.Infrastructure.Tests/
├── docker-compose.yml
├── docker-compose.override.yml
└── JobMarketIntelligence.sln
```

---

## Layer 1 — Domain

### Entities

```csharp
// Common/BaseEntity.cs
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}

// Common/AuditableEntity.cs
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public void SetUpdated() => UpdatedAt = DateTime.UtcNow;
}
```

```csharp
// Entities/Job.cs
public class Job : AuditableEntity
{
    public string Title { get; private set; }
    public string Company { get; private set; }
    public string Country { get; private set; }
    public string City { get; private set; }
    public string Description { get; private set; }
    public List<string> RequiredSkills { get; private set; } = new();
    public SeniorityLevel SeniorityLevel { get; private set; }
    public JobType JobType { get; private set; }
    public decimal? SalaryMin { get; private set; }
    public decimal? SalaryMax { get; private set; }
    public string Currency { get; private set; }
    public JobSource Source { get; private set; }
    public string SourceUrl { get; private set; }
    public string SourceJobId { get; private set; }
    public float[] Embedding { get; private set; }
    public EnrichmentStatus EnrichmentStatus { get; private set; }
    public DateTime PostedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    // Factory method — enforces invariants
    public static Job Create(string title, string company, string country,
        string city, string description, JobSource source,
        string sourceUrl, string sourceJobId, DateTime postedAt)
    {
        // guard clauses
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Title is required.");
        if (string.IsNullOrWhiteSpace(sourceUrl)) throw new DomainException("Source URL is required.");

        return new Job
        {
            Title = title,
            Company = company,
            Country = country,
            City = city,
            Description = description,
            Source = source,
            SourceUrl = sourceUrl,
            SourceJobId = sourceJobId,
            PostedAt = postedAt,
            EnrichmentStatus = EnrichmentStatus.Pending
        };
    }

    public void Enrich(List<string> skills, SeniorityLevel level,
        decimal? salaryMin, decimal? salaryMax, string currency)
    {
        RequiredSkills = skills;
        SeniorityLevel = level;
        SalaryMin = salaryMin;
        SalaryMax = salaryMax;
        Currency = currency;
        EnrichmentStatus = EnrichmentStatus.Enriched;
        SetUpdated();
    }

    public void SetEmbedding(float[] embedding)
    {
        Embedding = embedding;
        SetUpdated();
    }
}
```

```csharp
// Entities/CV.cs
public class CV : AuditableEntity
{
    public Guid UserId { get; private set; }
    public string OriginalFileName { get; private set; }
    public string StoragePath { get; private set; }
    public string RawText { get; private set; }
    public CvParsedData ParsedData { get; private set; }  // owned type / JSON column
    public float[] Embedding { get; private set; }
    public int? AtsScore { get; private set; }
    public AtsScoreBreakdown AtsBreakdown { get; private set; }
    public EnrichmentStatus EnrichmentStatus { get; private set; }

    public static CV Create(Guid userId, string originalFileName, string storagePath)
    {
        return new CV
        {
            UserId = userId,
            OriginalFileName = originalFileName,
            StoragePath = storagePath,
            EnrichmentStatus = EnrichmentStatus.Pending
        };
    }

    public void SetParsedData(CvParsedData parsedData, string rawText)
    {
        ParsedData = parsedData;
        RawText = rawText;
        EnrichmentStatus = EnrichmentStatus.Processing;
        SetUpdated();
    }

    public void SetEmbedding(float[] embedding)
    {
        Embedding = embedding;
        SetUpdated();
    }

    public void SetAtsScore(int score, AtsScoreBreakdown breakdown)
    {
        AtsScore = score;
        AtsBreakdown = breakdown;
        EnrichmentStatus = EnrichmentStatus.Enriched;
        SetUpdated();
    }
}
```

```csharp
// Entities/User.cs
public class User : AuditableEntity
{
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public string RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }
    public UserPreferences Preferences { get; private set; }

    public static User Create(string email, string passwordHash)
        => new User { Email = email, PasswordHash = passwordHash, Role = UserRole.JobSeeker };

    public void SetRefreshToken(string token, DateTime expiry)
    {
        RefreshToken = token;
        RefreshTokenExpiry = expiry;
    }

    public bool IsRefreshTokenValid(string token)
        => RefreshToken == token && RefreshTokenExpiry > DateTime.UtcNow;
}
```

```csharp
// Value Objects / Owned Types
public class CvParsedData
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Summary { get; set; }
    public List<string> Skills { get; set; } = new();
    public List<WorkExperience> Experience { get; set; } = new();
    public List<Education> Education { get; set; } = new();
    public List<string> Languages { get; set; } = new();
    public int TotalYearsExperience { get; set; }
}

public class WorkExperience
{
    public string Title { get; set; }
    public string Company { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Description { get; set; }
}

public class AtsScoreBreakdown
{
    public int KeywordScore { get; set; }        // 0-30
    public int FormattingScore { get; set; }     // 0-20
    public int CompletenessScore { get; set; }   // 0-25
    public int SkillRelevanceScore { get; set; } // 0-25
    public List<string> Suggestions { get; set; } = new();
}
```

### Enums

```csharp
public enum EnrichmentStatus { Pending, Processing, Enriched, Failed }
public enum JobSource { Indeed, Glassdoor, Wuzzuf, Adzuna, Remotive }
public enum SeniorityLevel { Intern, Junior, Mid, Senior, Lead, Principal }
public enum JobType { FullTime, PartTime, Contract, Freelance, Remote, Hybrid }
public enum UserRole { JobSeeker, Admin }
```

### Domain Exceptions

```csharp
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.") { }
}
```

---

## Layer 2 — Application

### Generic Repository & Unit of Work Interfaces

```csharp
// Interfaces/IRepository.cs
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
}

// Interfaces/IUnitOfWork.cs
public interface IUnitOfWork : IDisposable
{
    IRepository<Job> Jobs { get; }
    IRepository<CV> CVs { get; }
    IRepository<User> Users { get; }
    IRepository<JobAlert> JobAlerts { get; }
    IRepository<WatchlistItem> WatchlistItems { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
```

### AI & External Service Interfaces

```csharp
// Interfaces/IEmbeddingService.cs
public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default);
    Task<List<float[]>> GenerateBatchEmbeddingsAsync(List<string> texts, CancellationToken ct = default);
}

// Interfaces/ICvParsingService.cs
public interface ICvParsingService
{
    Task<CvParsedData> ParseAsync(string rawText, CancellationToken ct = default);
}

// Interfaces/IAtsService.cs
public interface IAtsService
{
    Task<(int Score, AtsScoreBreakdown Breakdown)> ScoreAsync(CvParsedData cvData, CancellationToken ct = default);
}

// Interfaces/IJobEnrichmentService.cs
public interface IJobEnrichmentService
{
    Task<JobEnrichmentResult> EnrichAsync(string title, string description, CancellationToken ct = default);
}

// Interfaces/IVectorSearchService.cs
public interface IVectorSearchService
{
    Task<List<JobMatchResult>> FindSimilarJobsAsync(float[] queryVector, int topK,
        JobSearchFilter? filter = null, CancellationToken ct = default);
}

// Interfaces/ICvTextExtractor.cs
public interface ICvTextExtractor
{
    Task<string> ExtractAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}

// Interfaces/IFileStorageService.cs
public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string storagePath, CancellationToken ct = default);
    Task DeleteAsync(string storagePath, CancellationToken ct = default);
}
```

### Scraper Strategy Interface

```csharp
// Interfaces/IScraperStrategy.cs
public interface IScraperStrategy
{
    JobSource Source { get; }
    Task<List<RawJobListing>> ScrapeAsync(ScraperOptions options, CancellationToken ct = default);
}

public record RawJobListing(
    string Title, string Company, string Country, string City,
    string Description, string SourceUrl, string SourceJobId,
    DateTime PostedAt, string? SalaryRaw);
```

### MediatR Pipeline Behaviors

```csharp
// Behaviors/ValidationBehavior.cs
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}

// Behaviors/LoggingBehavior.cs
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName}", requestName);
        var sw = Stopwatch.StartNew();
        var response = await next();
        sw.Stop();
        _logger.LogInformation("Handled {RequestName} in {ElapsedMs}ms", requestName, sw.ElapsedMilliseconds);
        return response;
    }
}
```

### Feature: CV Upload (Example Full Feature)

```csharp
// Features/CVs/Commands/UploadCv/UploadCvCommand.cs
public record UploadCvCommand(Guid UserId, Stream FileStream, string FileName) : IRequest<UploadCvResponse>;

// Features/CVs/Commands/UploadCv/UploadCvValidator.cs
public class UploadCvValidator : AbstractValidator<UploadCvCommand>
{
    private static readonly string[] AllowedExtensions = { ".pdf", ".docx" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public UploadCvValidator()
    {
        RuleFor(x => x.FileName)
            .Must(f => AllowedExtensions.Contains(Path.GetExtension(f).ToLower()))
            .WithMessage("Only PDF and DOCX files are allowed.");

        RuleFor(x => x.FileStream)
            .Must(s => s.Length <= MaxFileSize)
            .WithMessage("File size must not exceed 5MB.");
    }
}

// Features/CVs/Commands/UploadCv/UploadCvHandler.cs
public class UploadCvHandler : IRequestHandler<UploadCvCommand, UploadCvResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly IFileStorageService _storage;
    private readonly IBackgroundJobClient _jobs;
    private readonly IMapper _mapper;

    public UploadCvHandler(IUnitOfWork uow, IFileStorageService storage,
        IBackgroundJobClient jobs, IMapper mapper)
    {
        _uow = uow;
        _storage = storage;
        _jobs = jobs;
        _mapper = mapper;
    }

    public async Task<UploadCvResponse> Handle(UploadCvCommand request, CancellationToken ct)
    {
        var storagePath = await _storage.UploadAsync(request.FileStream, request.FileName, ct);
        var cv = CV.Create(request.UserId, request.FileName, storagePath);

        await _uow.CVs.AddAsync(cv, ct);
        await _uow.SaveChangesAsync(ct);

        // Enqueue background processing
        _jobs.Enqueue<ICvProcessingJob>(j => j.ProcessAsync(cv.Id, CancellationToken.None));

        return _mapper.Map<UploadCvResponse>(cv);
    }
}

// Features/CVs/DTOs/UploadCvResponse.cs
public record UploadCvResponse(Guid CvId, string Status, string Message);
```

### AutoMapper Profile Example

```csharp
// Mappings/CvProfile.cs
public class CvProfile : Profile
{
    public CvProfile()
    {
        CreateMap<CV, UploadCvResponse>()
            .ForMember(d => d.CvId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.EnrichmentStatus.ToString()))
            .ForMember(d => d.Message, o => o.Ignore());

        CreateMap<CV, CvDetailsResponse>()
            .ForMember(d => d.Skills, o => o.MapFrom(s => s.ParsedData != null
                ? s.ParsedData.Skills : new List<string>()));
    }
}

// Mappings/JobProfile.cs
public class JobProfile : Profile
{
    public JobProfile()
    {
        CreateMap<Job, JobSummaryDto>()
            .ForMember(d => d.Location, o => o.MapFrom(s => $"{s.City}, {s.Country}"))
            .ForMember(d => d.SalaryRange, o => o.MapFrom(s =>
                s.SalaryMin.HasValue ? $"{s.SalaryMin} - {s.SalaryMax} {s.Currency}" : "Not specified"));

        CreateMap<Job, JobDetailsDto>();
    }
}
```

---

## Layer 3 — Infrastructure

### EF Core DbContext

```csharp
// Persistence/AppDbContext.cs
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<CV> CVs => Set<CV>();
    public DbSet<User> Users => Set<User>();
    public DbSet<JobAlert> JobAlerts => Set<JobAlert>();
    public DbSet<WatchlistItem> WatchlistItems => Set<WatchlistItem>();
    public DbSet<SkillTrend> SkillTrends => Set<SkillTrend>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasPostgresExtension("vector");
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
```

### Entity Configurations (Fluent API)

```csharp
// Persistence/Configurations/JobConfiguration.cs
public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasKey(j => j.Id);
        builder.Property(j => j.Title).HasMaxLength(300).IsRequired();
        builder.Property(j => j.Company).HasMaxLength(200).IsRequired();
        builder.Property(j => j.SourceUrl).HasMaxLength(2000).IsRequired();
        builder.Property(j => j.SourceJobId).HasMaxLength(500).IsRequired();
        builder.HasIndex(j => new { j.Source, j.SourceJobId }).IsUnique(); // dedup index
        builder.Property(j => j.RequiredSkills).HasColumnType("jsonb");
        builder.Property(j => j.Embedding).HasColumnType("vector(1536)");
        builder.HasIndex(j => j.Embedding).HasMethod("ivfflat")
            .HasOperators("vector_cosine_ops")
            .HasStorageParameter("lists", 100); // pgvector IVFFlat index
    }
}

// Persistence/Configurations/CvConfiguration.cs
public class CvConfiguration : IEntityTypeConfiguration<CV>
{
    public void Configure(EntityTypeBuilder<CV> builder)
    {
        builder.HasKey(c => c.Id);
        builder.OwnsOne(c => c.ParsedData, pd =>
        {
            pd.ToJson(); // EF Core owned entity stored as JSON column
        });
        builder.OwnsOne(c => c.AtsBreakdown, ab => { ab.ToJson(); });
        builder.Property(c => c.Embedding).HasColumnType("vector(1536)");
    }
}
```

### Generic Repository Implementation

```csharp
// Persistence/Repositories/GenericRepository.cs
public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbSet.FindAsync(new object[] { id }, ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await _dbSet.ToListAsync(ct);

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.Where(predicate).ToListAsync(ct);

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
        return entity;
    }

    public Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => await _dbSet.AnyAsync(predicate, ct);

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
        => predicate == null
            ? await _dbSet.CountAsync(ct)
            : await _dbSet.CountAsync(predicate, ct);
}
```

### Unit of Work Implementation

```csharp
// Persistence/UnitOfWork.cs
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public IRepository<Job> Jobs { get; }
    public IRepository<CV> CVs { get; }
    public IRepository<User> Users { get; }
    public IRepository<JobAlert> JobAlerts { get; }
    public IRepository<WatchlistItem> WatchlistItems { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Jobs = new GenericRepository<Job>(context);
        CVs = new GenericRepository<CV>(context);
        Users = new GenericRepository<User>(context);
        JobAlerts = new GenericRepository<JobAlert>(context);
        WatchlistItems = new GenericRepository<WatchlistItem>(context);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await _context.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
        await _transaction!.CommitAsync(ct);
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
        => await _transaction!.RollbackAsync(ct);

    public void Dispose() => _context.Dispose();
}
```

### Scraper Strategy Pattern

```csharp
// Scrapers/ScraperFactory.cs
public class ScraperFactory
{
    private readonly Dictionary<JobSource, IScraperStrategy> _scrapers;

    public ScraperFactory(IEnumerable<IScraperStrategy> scrapers)
        => _scrapers = scrapers.ToDictionary(s => s.Source);

    public IScraperStrategy GetScraper(JobSource source)
        => _scrapers.TryGetValue(source, out var scraper)
            ? scraper
            : throw new InvalidOperationException($"No scraper registered for {source}");
}

// Scrapers/WuzzufScraperStrategy.cs
public class WuzzufScraperStrategy : IScraperStrategy
{
    public JobSource Source => JobSource.Wuzzuf;
    private readonly HttpClient _httpClient;
    private readonly ILogger<WuzzufScraperStrategy> _logger;

    public WuzzufScraperStrategy(HttpClient httpClient, ILogger<WuzzufScraperStrategy> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<RawJobListing>> ScrapeAsync(ScraperOptions options, CancellationToken ct)
    {
        // HtmlAgilityPack or AngleSharp for parsing
        // Polly retry built into named HttpClient
        throw new NotImplementedException();
    }
}
```

### OpenAI Embedding Implementation

```csharp
// AI/OpenAiEmbeddingService.cs
public class OpenAiEmbeddingService : IEmbeddingService
{
    private readonly OpenAIClient _client;
    private readonly ILogger<OpenAiEmbeddingService> _logger;
    private const string Model = "text-embedding-3-small";

    public OpenAiEmbeddingService(OpenAIClient client, ILogger<OpenAiEmbeddingService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        var response = await _client.GetEmbeddingsAsync(
            new EmbeddingsOptions(Model, new List<string> { text }), ct);
        return response.Value.Data[0].Embedding.ToArray();
    }

    public async Task<List<float[]>> GenerateBatchEmbeddingsAsync(List<string> texts, CancellationToken ct = default)
    {
        var response = await _client.GetEmbeddingsAsync(
            new EmbeddingsOptions(Model, texts), ct);
        return response.Value.Data.Select(d => d.Embedding.ToArray()).ToList();
    }
}
```

### Vector Search Service

```csharp
// Persistence/VectorSearchService.cs
public class VectorSearchService : IVectorSearchService
{
    private readonly AppDbContext _context;

    public VectorSearchService(AppDbContext context) => _context = context;

    public async Task<List<JobMatchResult>> FindSimilarJobsAsync(float[] queryVector, int topK,
        JobSearchFilter? filter = null, CancellationToken ct = default)
    {
        var vectorString = $"[{string.Join(",", queryVector)}]";

        var query = _context.Jobs
            .Where(j => j.EnrichmentStatus == EnrichmentStatus.Enriched)
            .Where(j => j.Embedding != null);

        if (filter?.Country != null)
            query = query.Where(j => j.Country == filter.Country);
        if (filter?.Category != null)
            query = query.Where(j => j.SeniorityLevel == filter.SeniorityLevel);

        // Raw SQL for cosine similarity via pgvector
        return await _context.Jobs
            .FromSqlRaw(
                "SELECT *, 1 - (embedding <=> {0}::vector) AS match_score FROM jobs ORDER BY embedding <=> {0}::vector LIMIT {1}",
                vectorString, topK)
            .Select(j => new JobMatchResult
            {
                Job = j,
                MatchScore = 0 // populated from raw SQL
            })
            .ToListAsync(ct);
    }
}
```

### Background Jobs

```csharp
// BackgroundJobs/CvProcessingJob.cs
public class CvProcessingJob : ICvProcessingJob
{
    private readonly IUnitOfWork _uow;
    private readonly ICvTextExtractor _extractor;
    private readonly ICvParsingService _parser;
    private readonly IEmbeddingService _embedding;
    private readonly IAtsService _ats;
    private readonly IFileStorageService _storage;
    private readonly ILogger<CvProcessingJob> _logger;

    public async Task ProcessAsync(Guid cvId, CancellationToken ct)
    {
        var cv = await _uow.CVs.GetByIdAsync(cvId, ct)
            ?? throw new NotFoundException(nameof(CV), cvId);

        try
        {
            // Step 1: Extract text
            var fileStream = await _storage.DownloadAsync(cv.StoragePath, ct);
            var rawText = await _extractor.ExtractAsync(fileStream, cv.OriginalFileName, ct);

            // Step 2: Parse with AI
            var parsedData = await _parser.ParseAsync(rawText, ct);
            cv.SetParsedData(parsedData, rawText);

            // Step 3: Generate embedding
            var embeddingText = BuildEmbeddingText(parsedData);
            var embedding = await _embedding.GenerateEmbeddingAsync(embeddingText, ct);
            cv.SetEmbedding(embedding);

            // Step 4: ATS scoring
            var (score, breakdown) = await _ats.ScoreAsync(parsedData, ct);
            cv.SetAtsScore(score, breakdown);

            await _uow.SaveChangesAsync(ct);
            _logger.LogInformation("CV {CvId} processed successfully. ATS Score: {Score}", cvId, score);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process CV {CvId}", cvId);
            throw; // Hangfire will retry
        }
    }

    private static string BuildEmbeddingText(CvParsedData data)
        => $"Skills: {string.Join(", ", data.Skills)} " +
           $"Titles: {string.Join(", ", data.Experience.Select(e => e.Title))} " +
           $"Summary: {data.Summary} " +
           $"Experience: {data.TotalYearsExperience} years";
}
```

---

## Layer 4 — Presentation (API)

### Controller Example

```csharp
// Controllers/CvController.cs
[ApiController]
[Route("api/cv")]
[Authorize]
public class CvController : ControllerBase
{
    private readonly ISender _mediator;

    public CvController(ISender mediator) => _mediator = mediator;

    [HttpPost("upload")]
    public async Task<ActionResult<UploadCvResponse>> Upload(IFormFile file, CancellationToken ct)
    {
        var userId = User.GetUserId(); // extension method reading JWT claim
        var command = new UploadCvCommand(userId, file.OpenReadStream(), file.FileName);
        var result = await _mediator.Send(command, ct);
        return Accepted(result);
    }

    [HttpGet("{id}/matches")]
    public async Task<ActionResult<List<JobMatchDto>>> GetMatches(
        Guid id, [FromQuery] JobMatchFilter filter, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var query = new GetJobMatchesQuery(id, userId, filter);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id}/ats-score")]
    public async Task<ActionResult<AtsScoreResponse>> GetAtsScore(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var query = new GetAtsScoreQuery(id, userId);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }
}
```

### Global Exception Handling Middleware

```csharp
// Middleware/ExceptionHandlingMiddleware.cs
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
            });
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { message = "An unexpected error occurred." });
        }
    }
}
```

### DI Registration

```csharp
// Extensions/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
            typeof(ApplicationAssemblyMarker).Assembly));
        services.AddAutoMapper(typeof(ApplicationAssemblyMarker).Assembly);
        services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(config.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.UseVector()));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEmbeddingService, OpenAiEmbeddingService>();
        services.AddScoped<ICvParsingService, OpenAiCvParsingService>();
        services.AddScoped<IAtsService, AtsService>();
        services.AddScoped<IVectorSearchService, VectorSearchService>();
        services.AddScoped<ICvTextExtractor, PdfPigTextExtractor>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // Scraper strategies
        services.AddScoped<IScraperStrategy, WuzzufScraperStrategy>();
        services.AddScoped<IScraperStrategy, IndeedScraperStrategy>();
        services.AddScoped<IScraperStrategy, GlassdoorScraperStrategy>();
        services.AddScoped<IScraperStrategy, AdzunaApiStrategy>();
        services.AddScoped<IScraperStrategy, RemotiveApiStrategy>();
        services.AddScoped<ScraperFactory>();

        services.AddHangfire(cfg => cfg.UsePostgreSqlStorage(
            config.GetConnectionString("DefaultConnection")));
        services.AddHangfireServer();

        return services;
    }
}
```

---

## Database Schema (Key Tables)

```sql
CREATE EXTENSION IF NOT EXISTS vector;

CREATE TABLE jobs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(300) NOT NULL,
    company VARCHAR(200) NOT NULL,
    country VARCHAR(100),
    city VARCHAR(100),
    description TEXT,
    required_skills JSONB DEFAULT '[]',
    seniority_level INT,
    job_type INT,
    salary_min DECIMAL,
    salary_max DECIMAL,
    currency VARCHAR(10),
    source INT NOT NULL,
    source_url VARCHAR(2000) NOT NULL,
    source_job_id VARCHAR(500) NOT NULL,
    embedding vector(1536),
    enrichment_status INT NOT NULL DEFAULT 0,
    posted_at TIMESTAMPTZ NOT NULL,
    expires_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    UNIQUE (source, source_job_id)
);

CREATE INDEX ON jobs USING ivfflat (embedding vector_cosine_ops) WITH (lists = 100);

CREATE TABLE cvs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id),
    original_file_name VARCHAR(500),
    storage_path VARCHAR(1000),
    raw_text TEXT,
    parsed_data JSONB,
    ats_breakdown JSONB,
    embedding vector(1536),
    ats_score INT,
    enrichment_status INT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ
);
```

---

## Docker Compose

```yaml
services:
  api:
    build: .
    ports:
      - "5000:8080"
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=jobmarket;Username=postgres;Password=postgres
      - OpenAI__ApiKey=${OPENAI_API_KEY}
    depends_on:
      - postgres
      - redis

  postgres:
    image: pgvector/pgvector:pg16
    environment:
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: jobmarket
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"

  redis:
    image: redis:latest
    ports:
      - "6379:6379"

  hangfire-dashboard:
    image: jobmarket-api
    ports:
      - "5001:8080"
    command: ["--hangfire-dashboard"]

volumes:
  postgres_data:
```
