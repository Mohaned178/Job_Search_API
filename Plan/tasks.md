# Job Market Intelligence API — Task List

## How to Use This Document

Tasks are organized by phase. Complete each phase fully before moving to the next. Every task maps to a specific file or set of files. Check off tasks as you complete them.

**Priority Legend**
- `[MUST]` — Required for the phase to be considered complete
- `[SHOULD]` — Important but can be deferred one phase
- `[NICE]` — Enhancement, implement after core is stable

---

## Phase 1 — Solution Foundation

### 1.1 Solution Setup
- [ ] `[MUST]` Create solution file `JobMarketIntelligence.sln`
- [ ] `[MUST]` Create project `JobMarket.Domain` (Class Library)
- [ ] `[MUST]` Create project `JobMarket.Application` (Class Library)
- [ ] `[MUST]` Create project `JobMarket.Infrastructure` (Class Library)
- [ ] `[MUST]` Create project `JobMarket.API` (ASP.NET Core Web API)
- [ ] `[MUST]` Create project `JobMarket.Application.Tests` (xUnit)
- [ ] `[MUST]` Add project references: API → Infrastructure → Application → Domain
- [ ] `[MUST]` Add `.gitignore`, `.editorconfig`, `README.md`

### 1.2 NuGet Packages

**Domain** — no external packages

**Application**
- [ ] `[MUST]` `MediatR`
- [ ] `[MUST]` `FluentValidation`
- [ ] `[MUST]` `AutoMapper`
- [ ] `[MUST]` `Microsoft.Extensions.Logging.Abstractions`

**Infrastructure**
- [ ] `[MUST]` `Microsoft.EntityFrameworkCore`
- [ ] `[MUST]` `Npgsql.EntityFrameworkCore.PostgreSQL`
- [ ] `[MUST]` `Pgvector.EntityFrameworkCore`
- [ ] `[MUST]` `Hangfire.Core`
- [ ] `[MUST]` `Hangfire.PostgreSql`
- [ ] `[MUST]` `StackExchange.Redis`
- [ ] `[MUST]` `OpenAI` (official SDK)
- [ ] `[MUST]` `PdfPig`
- [ ] `[MUST]` `HtmlAgilityPack`
- [ ] `[MUST]` `Polly`
- [ ] `[MUST]` `Microsoft.Extensions.Http.Polly`
- [ ] `[SHOULD]` `AngleSharp`

**API**
- [ ] `[MUST]` `Microsoft.AspNetCore.Authentication.JwtBearer`
- [ ] `[MUST]` `Serilog.AspNetCore`
- [ ] `[MUST]` `Serilog.Sinks.Console`
- [ ] `[MUST]` `Scalar.AspNetCore`
- [ ] `[MUST]` `Hangfire.AspNetCore`

**Tests**
- [ ] `[MUST]` `xunit`
- [ ] `[MUST]` `Moq`
- [ ] `[MUST]` `FluentAssertions`
- [ ] `[MUST]` `Microsoft.EntityFrameworkCore.InMemory`

---

## Phase 2 — Domain Layer

### 2.1 Common Base Classes
- [ ] `[MUST]` Create `Domain/Common/BaseEntity.cs` — Id (Guid)
- [ ] `[MUST]` Create `Domain/Common/AuditableEntity.cs` — CreatedAt, UpdatedAt, SetUpdated()

### 2.2 Enums
- [ ] `[MUST]` Create `Domain/Enums/EnrichmentStatus.cs` — Pending, Processing, Enriched, Failed
- [ ] `[MUST]` Create `Domain/Enums/JobSource.cs` — Indeed, Glassdoor, Wuzzuf, Adzuna, Remotive
- [ ] `[MUST]` Create `Domain/Enums/SeniorityLevel.cs` — Intern, Junior, Mid, Senior, Lead, Principal
- [ ] `[MUST]` Create `Domain/Enums/JobType.cs` — FullTime, PartTime, Contract, Freelance, Remote, Hybrid
- [ ] `[MUST]` Create `Domain/Enums/UserRole.cs` — JobSeeker, Admin

### 2.3 Value Objects
- [ ] `[MUST]` Create `Domain/ValueObjects/CvParsedData.cs`
- [ ] `[MUST]` Create `Domain/ValueObjects/WorkExperience.cs`
- [ ] `[MUST]` Create `Domain/ValueObjects/Education.cs`
- [ ] `[MUST]` Create `Domain/ValueObjects/AtsScoreBreakdown.cs`

### 2.4 Entities
- [ ] `[MUST]` Create `Domain/Entities/Job.cs` with factory method `Create()` and methods `Enrich()`, `SetEmbedding()`
- [ ] `[MUST]` Create `Domain/Entities/CV.cs` with `Create()`, `SetParsedData()`, `SetEmbedding()`, `SetAtsScore()`
- [ ] `[MUST]` Create `Domain/Entities/User.cs` with `Create()`, `SetRefreshToken()`, `IsRefreshTokenValid()`
- [ ] `[MUST]` Create `Domain/Entities/UserPreferences.cs`
- [ ] `[MUST]` Create `Domain/Entities/JobAlert.cs`
- [ ] `[MUST]` Create `Domain/Entities/WatchlistItem.cs`
- [ ] `[MUST]` Create `Domain/Entities/SkillTrend.cs`

### 2.5 Exceptions
- [ ] `[MUST]` Create `Domain/Exceptions/DomainException.cs`
- [ ] `[MUST]` Create `Domain/Exceptions/NotFoundException.cs`
- [ ] `[MUST]` Create `Domain/Exceptions/UnauthorizedException.cs`

---

## Phase 3 — Application Layer

### 3.1 Interfaces
- [ ] `[MUST]` Create `Application/Common/Interfaces/IRepository.cs` — generic CRUD + Exists + Count
- [ ] `[MUST]` Create `Application/Common/Interfaces/IUnitOfWork.cs` — typed repos + SaveChanges + transaction methods
- [ ] `[MUST]` Create `Application/Common/Interfaces/IEmbeddingService.cs`
- [ ] `[MUST]` Create `Application/Common/Interfaces/ICvParsingService.cs`
- [ ] `[MUST]` Create `Application/Common/Interfaces/IAtsService.cs`
- [ ] `[MUST]` Create `Application/Common/Interfaces/IJobEnrichmentService.cs`
- [ ] `[MUST]` Create `Application/Common/Interfaces/IVectorSearchService.cs`
- [ ] `[MUST]` Create `Application/Common/Interfaces/ICvTextExtractor.cs`
- [ ] `[MUST]` Create `Application/Common/Interfaces/IFileStorageService.cs`
- [ ] `[MUST]` Create `Application/Common/Interfaces/IScraperStrategy.cs`
- [ ] `[MUST]` Create `Application/Common/Interfaces/ITokenService.cs`
- [ ] `[MUST]` Create `Application/Common/Interfaces/ICvProcessingJob.cs`
- [ ] `[MUST]` Create `Application/Common/Interfaces/IJobIngestionJob.cs`

### 3.2 Pipeline Behaviors
- [ ] `[MUST]` Create `Application/Common/Behaviors/ValidationBehavior.cs`
- [ ] `[MUST]` Create `Application/Common/Behaviors/LoggingBehavior.cs`
- [ ] `[SHOULD]` Create `Application/Common/Behaviors/PerformanceBehavior.cs` — warn if handler > 500ms

### 3.3 AutoMapper Profiles
- [ ] `[MUST]` Create `Application/Common/Mappings/JobProfile.cs`
- [ ] `[MUST]` Create `Application/Common/Mappings/CvProfile.cs`
- [ ] `[MUST]` Create `Application/Common/Mappings/UserProfile.cs`

### 3.4 Feature — Authentication
- [ ] `[MUST]` Create `Application/Features/Auth/Commands/Register/RegisterCommand.cs`
- [ ] `[MUST]` Create `Application/Features/Auth/Commands/Register/RegisterValidator.cs`
- [ ] `[MUST]` Create `Application/Features/Auth/Commands/Register/RegisterHandler.cs`
- [ ] `[MUST]` Create `Application/Features/Auth/Commands/Login/LoginCommand.cs`
- [ ] `[MUST]` Create `Application/Features/Auth/Commands/Login/LoginValidator.cs`
- [ ] `[MUST]` Create `Application/Features/Auth/Commands/Login/LoginHandler.cs`
- [ ] `[MUST]` Create `Application/Features/Auth/Commands/RefreshToken/RefreshTokenCommand.cs`
- [ ] `[MUST]` Create `Application/Features/Auth/Commands/RefreshToken/RefreshTokenHandler.cs`
- [ ] `[MUST]` Create `Application/Features/Auth/DTOs/AuthResponse.cs`

### 3.5 Feature — Jobs
- [ ] `[MUST]` Create `Application/Features/Jobs/Queries/SearchJobs/SearchJobsQuery.cs`
- [ ] `[MUST]` Create `Application/Features/Jobs/Queries/SearchJobs/SearchJobsValidator.cs`
- [ ] `[MUST]` Create `Application/Features/Jobs/Queries/SearchJobs/SearchJobsHandler.cs`
- [ ] `[MUST]` Create `Application/Features/Jobs/Queries/GetJobById/GetJobByIdQuery.cs`
- [ ] `[MUST]` Create `Application/Features/Jobs/Queries/GetJobById/GetJobByIdHandler.cs`
- [ ] `[MUST]` Create `Application/Features/Jobs/Queries/GetRelatedJobs/GetRelatedJobsQuery.cs`
- [ ] `[MUST]` Create `Application/Features/Jobs/Queries/GetRelatedJobs/GetRelatedJobsHandler.cs`
- [ ] `[MUST]` Create `Application/Features/Jobs/Queries/GetTrendingJobs/GetTrendingJobsQuery.cs`
- [ ] `[MUST]` Create `Application/Features/Jobs/Queries/GetTrendingJobs/GetTrendingJobsHandler.cs`
- [ ] `[MUST]` Create `Application/Features/Jobs/DTOs/JobSummaryDto.cs`
- [ ] `[MUST]` Create `Application/Features/Jobs/DTOs/JobDetailsDto.cs`
- [ ] `[MUST]` Create `Application/Features/Jobs/DTOs/JobSearchFilter.cs`
- [ ] `[MUST]` Create `Application/Features/Jobs/DTOs/PaginatedResult.cs`

### 3.6 Feature — CV Intelligence
- [ ] `[MUST]` Create `Application/Features/CVs/Commands/UploadCv/UploadCvCommand.cs`
- [ ] `[MUST]` Create `Application/Features/CVs/Commands/UploadCv/UploadCvValidator.cs`
- [ ] `[MUST]` Create `Application/Features/CVs/Commands/UploadCv/UploadCvHandler.cs`
- [ ] `[MUST]` Create `Application/Features/CVs/Commands/DeleteCv/DeleteCvCommand.cs`
- [ ] `[MUST]` Create `Application/Features/CVs/Commands/DeleteCv/DeleteCvHandler.cs`
- [ ] `[MUST]` Create `Application/Features/CVs/Queries/GetCvStatus/GetCvStatusQuery.cs`
- [ ] `[MUST]` Create `Application/Features/CVs/Queries/GetCvStatus/GetCvStatusHandler.cs`
- [ ] `[MUST]` Create `Application/Features/CVs/Queries/GetAtsScore/GetAtsScoreQuery.cs`
- [ ] `[MUST]` Create `Application/Features/CVs/Queries/GetAtsScore/GetAtsScoreHandler.cs`
- [ ] `[MUST]` Create `Application/Features/CVs/Queries/GetJobMatches/GetJobMatchesQuery.cs`
- [ ] `[MUST]` Create `Application/Features/CVs/Queries/GetJobMatches/GetJobMatchesHandler.cs`
- [ ] `[MUST]` Create `Application/Features/CVs/Queries/GetGapAnalysis/GetGapAnalysisQuery.cs`
- [ ] `[MUST]` Create `Application/Features/CVs/Queries/GetGapAnalysis/GetGapAnalysisHandler.cs`
- [ ] `[MUST]` Create `Application/Features/CVs/Queries/GetRoadmap/GetRoadmapQuery.cs`
- [ ] `[MUST]` Create `Application/Features/CVs/Queries/GetRoadmap/GetRoadmapHandler.cs`
- [ ] `[MUST]` Create `Application/Features/CVs/DTOs/UploadCvResponse.cs`
- [ ] `[MUST]` Create `Application/Features/CVs/DTOs/CvStatusResponse.cs`
- [ ] `[MUST]` Create `Application/Features/CVs/DTOs/AtsScoreResponse.cs`
- [ ] `[MUST]` Create `Application/Features/CVs/DTOs/JobMatchDto.cs`
- [ ] `[MUST]` Create `Application/Features/CVs/DTOs/GapAnalysisResponse.cs`
- [ ] `[MUST]` Create `Application/Features/CVs/DTOs/RoadmapResponse.cs`

### 3.7 Feature — Analytics
- [ ] `[MUST]` Create `Application/Features/Analytics/Queries/GetTrendingSkills/GetTrendingSkillsQuery.cs`
- [ ] `[MUST]` Create `Application/Features/Analytics/Queries/GetTrendingSkills/GetTrendingSkillsHandler.cs`
- [ ] `[MUST]` Create `Application/Features/Analytics/Queries/GetSalaryIntelligence/GetSalaryIntelligenceQuery.cs`
- [ ] `[MUST]` Create `Application/Features/Analytics/Queries/GetSalaryIntelligence/GetSalaryIntelligenceHandler.cs`
- [ ] `[SHOULD]` Create `Application/Features/Analytics/Queries/GetMarketSummary/GetMarketSummaryQuery.cs`
- [ ] `[SHOULD]` Create `Application/Features/Analytics/Queries/GetMarketSummary/GetMarketSummaryHandler.cs`

### 3.8 Feature — Preferences & Alerts
- [ ] `[MUST]` Create `Application/Features/Preferences/Commands/UpdatePreferences/UpdatePreferencesCommand.cs`
- [ ] `[MUST]` Create `Application/Features/Preferences/Commands/UpdatePreferences/UpdatePreferencesHandler.cs`
- [ ] `[MUST]` Create `Application/Features/Preferences/Queries/GetPersonalizedFeed/GetPersonalizedFeedQuery.cs`
- [ ] `[MUST]` Create `Application/Features/Preferences/Queries/GetPersonalizedFeed/GetPersonalizedFeedHandler.cs`
- [ ] `[SHOULD]` Create `Application/Features/Alerts/Commands/CreateAlert/CreateAlertCommand.cs`
- [ ] `[SHOULD]` Create `Application/Features/Alerts/Commands/CreateAlert/CreateAlertHandler.cs`
- [ ] `[SHOULD]` Create `Application/Features/Watchlist/Commands/SaveJob/SaveJobCommand.cs`
- [ ] `[SHOULD]` Create `Application/Features/Watchlist/Commands/SaveJob/SaveJobHandler.cs`

---

## Phase 4 — Infrastructure Layer

### 4.1 Database Context
- [ ] `[MUST]` Create `Infrastructure/Persistence/AppDbContext.cs` — register all DbSets, enable pgvector extension
- [ ] `[MUST]` Create `Infrastructure/Persistence/Configurations/JobConfiguration.cs` — unique index on (Source, SourceJobId), IVFFlat vector index
- [ ] `[MUST]` Create `Infrastructure/Persistence/Configurations/CvConfiguration.cs` — JSON owned types for ParsedData and AtsBreakdown
- [ ] `[MUST]` Create `Infrastructure/Persistence/Configurations/UserConfiguration.cs`
- [ ] `[MUST]` Create `Infrastructure/Persistence/Configurations/UserPreferencesConfiguration.cs`
- [ ] `[MUST]` Create `Infrastructure/Persistence/Configurations/JobAlertConfiguration.cs`
- [ ] `[MUST]` Create `Infrastructure/Persistence/Configurations/WatchlistConfiguration.cs`

### 4.2 Repository & Unit of Work
- [ ] `[MUST]` Create `Infrastructure/Persistence/Repositories/GenericRepository.cs`
- [ ] `[MUST]` Create `Infrastructure/Persistence/UnitOfWork.cs`

### 4.3 Migrations
- [ ] `[MUST]` Run `dotnet ef migrations add InitialCreate`
- [ ] `[MUST]` Verify migration includes `CREATE EXTENSION IF NOT EXISTS vector` — add manually if missing
- [ ] `[MUST]` Verify IVFFlat index creation in migration
- [ ] `[MUST]` Run `dotnet ef database update`

### 4.4 AI Services
- [ ] `[MUST]` Create `Infrastructure/AI/OpenAiEmbeddingService.cs` — implements `IEmbeddingService`
- [ ] `[MUST]` Create `Infrastructure/AI/OpenAiCvParsingService.cs` — implements `ICvParsingService`, returns structured JSON via prompt
- [ ] `[MUST]` Create `Infrastructure/AI/AtsService.cs` — implements `IAtsService`, scores CV dimensions
- [ ] `[MUST]` Create `Infrastructure/AI/OpenAiJobEnrichmentService.cs` — implements `IJobEnrichmentService`
- [ ] `[MUST]` Create `Infrastructure/AI/VectorSearchService.cs` — implements `IVectorSearchService` using raw pgvector SQL

### 4.5 CV Text Extraction
- [ ] `[MUST]` Create `Infrastructure/Parsing/PdfPigTextExtractor.cs` — implements `ICvTextExtractor` for PDF
- [ ] `[SHOULD]` Create `Infrastructure/Parsing/DocxTextExtractor.cs` — implements `ICvTextExtractor` for DOCX
- [ ] `[MUST]` Create `Infrastructure/Parsing/CvTextExtractorFactory.cs` — selects extractor based on file extension

### 4.6 File Storage
- [ ] `[MUST]` Create `Infrastructure/Storage/LocalFileStorageService.cs` — implements `IFileStorageService` (stores to local disk for development)
- [ ] `[SHOULD]` Create `Infrastructure/Storage/S3FileStorageService.cs` — for production

### 4.7 Scrapers — Strategy Pattern
- [ ] `[MUST]` Create `Infrastructure/Scrapers/ScraperFactory.cs`
- [ ] `[MUST]` Create `Infrastructure/Scrapers/ScraperOptions.cs`
- [ ] `[MUST]` Create `Infrastructure/Scrapers/WuzzufScraperStrategy.cs` — implements `IScraperStrategy`
- [ ] `[MUST]` Create `Infrastructure/Scrapers/IndeedScraperStrategy.cs` — implements `IScraperStrategy`
- [ ] `[SHOULD]` Create `Infrastructure/Scrapers/GlassdoorScraperStrategy.cs` — implements `IScraperStrategy`
- [ ] `[MUST]` Create `Infrastructure/ExternalAPIs/AdzunaApiStrategy.cs` — implements `IScraperStrategy` using Adzuna REST API
- [ ] `[MUST]` Create `Infrastructure/ExternalAPIs/RemotiveApiStrategy.cs` — implements `IScraperStrategy` using Remotive REST API
- [ ] `[MUST]` Register all scrapers with typed HttpClients + Polly retry policy (3 retries, exponential backoff)

### 4.8 Background Jobs
- [ ] `[MUST]` Create `Infrastructure/BackgroundJobs/CvProcessingJob.cs` — implements `ICvProcessingJob`
  - Extract text from file
  - Parse with AI
  - Generate embedding
  - Score ATS
  - Save all results
- [ ] `[MUST]` Create `Infrastructure/BackgroundJobs/JobIngestionJob.cs` — implements `IJobIngestionJob`
  - Call each scraper strategy
  - Deduplicate against existing SourceJobId
  - Save new jobs
  - Enqueue enrichment for new jobs
- [ ] `[MUST]` Create `Infrastructure/BackgroundJobs/JobEnrichmentJob.cs`
  - Process jobs with EnrichmentStatus = Pending
  - Extract skills via AI
  - Generate embedding
  - Save enriched data
- [ ] `[SHOULD]` Create `Infrastructure/BackgroundJobs/AlertProcessingJob.cs`
  - Check new jobs against active alerts
  - Send notifications

### 4.9 Auth
- [ ] `[MUST]` Create `Infrastructure/Auth/TokenService.cs` — implements `ITokenService`, generates JWT and refresh tokens
- [ ] `[MUST]` Create `Infrastructure/Auth/PasswordHasher.cs` — BCrypt wrapper

### 4.10 Caching
- [ ] `[SHOULD]` Create `Infrastructure/Caching/RedisCacheService.cs`
- [ ] `[SHOULD]` Create `Infrastructure/Caching/CachedJobRepository.cs` — Decorator pattern over `IRepository<Job>`

### 4.11 DI Registration
- [ ] `[MUST]` Create `Infrastructure/DependencyInjection.cs` — `AddInfrastructure()` extension method registering all services
- [ ] `[MUST]` Create `Application/DependencyInjection.cs` — `AddApplication()` extension method

---

## Phase 5 — API Layer

### 5.1 Configuration
- [ ] `[MUST]` Configure `appsettings.json` with sections: `ConnectionStrings`, `Jwt`, `OpenAI`, `Hangfire`, `Redis`, `Scrapers`
- [ ] `[MUST]` Create `appsettings.Development.json` with local values
- [ ] `[MUST]` Create `Infrastructure/Options/JwtOptions.cs`, `OpenAiOptions.cs`, `ScraperOptions.cs`
- [ ] `[MUST]` Register all options via `services.Configure<T>(config.GetSection("..."))`

### 5.2 Middleware
- [ ] `[MUST]` Create `API/Middleware/ExceptionHandlingMiddleware.cs` — handles DomainException, NotFoundException, ValidationException, generic 500
- [ ] `[MUST]` Register middleware in `Program.cs`

### 5.3 Controllers
- [ ] `[MUST]` Create `API/Controllers/AuthController.cs` — Register, Login, Refresh, Logout
- [ ] `[MUST]` Create `API/Controllers/JobsController.cs` — Search, GetById, GetRelated, GetTrending
- [ ] `[MUST]` Create `API/Controllers/CvController.cs` — Upload, GetStatus, GetAtsScore, GetMatches, GetGapAnalysis, GetRoadmap, Delete
- [ ] `[MUST]` Create `API/Controllers/AnalyticsController.cs` — TrendingSkills, SalaryIntelligence
- [ ] `[SHOULD]` Create `API/Controllers/PreferencesController.cs` — GetPreferences, UpdatePreferences, GetFeed
- [ ] `[SHOULD]` Create `API/Controllers/WatchlistController.cs` — SaveJob, GetWatchlist, RemoveJob
- [ ] `[SHOULD]` Create `API/Controllers/AlertsController.cs` — CreateAlert, GetAlerts, DeleteAlert
- [ ] `[MUST]` Create `API/Controllers/AdminController.cs` — TriggerScrape, GetStats

### 5.4 Extensions & Helpers
- [ ] `[MUST]` Create `API/Extensions/ClaimsPrincipalExtensions.cs` — `GetUserId()` extension method
- [ ] `[MUST]` Create `API/Filters/AuthorizeRolesAttribute.cs`

### 5.5 Program.cs Wiring
- [ ] `[MUST]` Call `services.AddApplication()`
- [ ] `[MUST]` Call `services.AddInfrastructure(config)`
- [ ] `[MUST]` Configure JWT Bearer authentication
- [ ] `[MUST]` Configure CORS
- [ ] `[MUST]` Add Hangfire server and dashboard
- [ ] `[MUST]` Register Serilog
- [ ] `[MUST]` Add Scalar / Swagger
- [ ] `[MUST]` Register Hangfire recurring jobs on startup (JobIngestionJob every 6 hours)

---

## Phase 6 — Testing

### 6.1 Domain Tests
- [ ] `[MUST]` Test `Job.Create()` — valid input, missing title, missing source URL
- [ ] `[MUST]` Test `Job.Enrich()` — enrichment status transitions
- [ ] `[MUST]` Test `CV.SetAtsScore()` — score stored, status becomes Enriched
- [ ] `[MUST]` Test `User.IsRefreshTokenValid()` — valid token, expired token, wrong token

### 6.2 Application Handler Tests
- [ ] `[MUST]` Test `UploadCvHandler` — file saved, background job enqueued, CV created
- [ ] `[MUST]` Test `GetJobMatchesHandler` — CV not found throws NotFoundException, unauthorized user throws UnauthorizedException
- [ ] `[MUST]` Test `SearchJobsHandler` — filters applied correctly, pagination works
- [ ] `[MUST]` Test `RegisterHandler` — duplicate email throws, password hashed
- [ ] `[MUST]` Test `LoginHandler` — wrong password returns null, valid credentials return tokens
- [ ] `[SHOULD]` Test `GetGapAnalysisHandler` — missing skills correctly identified
- [ ] `[SHOULD]` Test `GetRoadmapHandler` — roadmap ordered by market demand frequency

### 6.3 Validator Tests
- [ ] `[MUST]` Test `UploadCvValidator` — rejects non-PDF/DOCX, rejects > 5MB
- [ ] `[MUST]` Test `RegisterValidator` — invalid email, password too short
- [ ] `[MUST]` Test `SearchJobsValidator` — invalid page size, invalid seniority value

---

## Phase 7 — Docker & Deployment

### 7.1 Docker
- [ ] `[MUST]` Create `Dockerfile` (multi-stage: build + runtime)
- [ ] `[MUST]` Create `docker-compose.yml` with: API, PostgreSQL (pgvector image), Redis
- [ ] `[MUST]` Create `docker-compose.override.yml` for dev overrides
- [ ] `[MUST]` Test `docker compose up` — all services start, migrations run on startup

### 7.2 Database Migrations on Startup
- [ ] `[MUST]` Add `app.MigrateDatabase()` extension in `Program.cs` that runs pending migrations automatically on startup

### 7.3 Health Checks
- [ ] `[SHOULD]` Add `/health` endpoint checking: database connectivity, Redis connectivity
- [ ] `[SHOULD]` Add `/health/ready` and `/health/live` endpoints for container orchestration

---

## Phase 8 — Polish & Documentation

- [ ] `[MUST]` Add XML documentation comments to all public interfaces
- [ ] `[MUST]` Add Scalar/Swagger descriptions and examples to all controller actions
- [ ] `[MUST]` Write `README.md` with: project overview, how to run locally, environment variables reference, API endpoint summary
- [ ] `[SHOULD]` Add `CONTRIBUTING.md` with branching strategy and definition of done
- [ ] `[NICE]` Create Postman collection for all endpoints
- [ ] `[NICE]` Add GitHub Actions CI workflow: build, test, Docker build on PR

---

## Implementation Order Summary

```
Phase 1 (Foundation)     → 1–2 days
Phase 2 (Domain)         → 1 day
Phase 3 (Application)    → 3–4 days
Phase 4 (Infrastructure) → 4–5 days   ← most complex phase
Phase 5 (API)            → 2 days
Phase 6 (Testing)        → 2 days
Phase 7 (Docker)         → 1 day
Phase 8 (Polish)         → 1 day
─────────────────────────────────────
Total estimate            15–18 days
```

---

## Key Decisions to Make Before Starting

1. **File storage** — local disk for MVP or S3 from day one?
2. **AI provider** — OpenAI only or abstract from day one to allow Claude as fallback?
3. **Scraping approach** — HtmlAgilityPack or Playwright (needed for JS-rendered pages like Indeed)?
4. **Notifications** — email (SendGrid) or in-app only for MVP?
5. **Rate limiting** — per-user IP or JWT-based? Decide before wiring auth.
