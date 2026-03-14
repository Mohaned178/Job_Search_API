# Job Market Intelligence API — Constitution

## Project Vision

A production-grade backend API that aggregates job listings from multiple international and regional sources, enriches them with AI-powered analysis, and delivers intelligent career guidance to job seekers. The system enables users to upload their CV and receive job matching, ATS scoring, skill gap analysis, and personalized learning roadmaps — all driven by semantic search and real market data.

---

## Core Principles

### 1. Clean Architecture is Non-Negotiable
Every feature must respect the four-layer boundary:
- **Domain** — pure business entities and logic, zero dependencies
- **Application** — use cases, interfaces, DTOs, CQRS handlers
- **Infrastructure** — EF Core, external APIs, scrapers, AI clients, Redis
- **Presentation** — ASP.NET Core controllers, middleware, filters

No layer may reference a layer above it. Infrastructure never leaks into Application. Domain never references EF Core.

### 2. Dependency Rule
All dependencies point inward. Domain knows nothing. Application knows only Domain. Infrastructure implements Application interfaces. Presentation orchestrates Application.

### 3. CQRS with MediatR
Every operation is either a Command (mutates state) or a Query (reads state). No service class handles both. All handlers live in the Application layer and are registered automatically via MediatR.

### 4. Generic Unit of Work + Repository Pattern
A generic `IRepository<T>` and `IUnitOfWork` are defined in Application and implemented in Infrastructure. No raw DbContext access outside Infrastructure. All database transactions go through UnitOfWork.

### 5. AutoMapper for All Transformations
Every mapping between Domain entities and DTOs/ViewModels goes through AutoMapper profiles. No manual mapping code in handlers or controllers. Profiles are organized per feature/module.

### 6. Design Patterns — Right Tool for the Right Problem
| Pattern | Where Used |
|---|---|
| Repository + Unit of Work | All data access |
| CQRS + MediatR | All use cases |
| Strategy | Scraper implementations, AI provider selection |
| Factory | Scraper factory, embedding provider factory |
| Observer / MediatR Notifications | Domain events (CV processed, job enriched) |
| Decorator | Caching layer over repositories |
| Pipeline Behavior | Validation, logging, exception handling in MediatR |
| Options Pattern | All external config (API keys, URLs, timeouts) |

### 7. Fail Fast with Validation
Every command and query is validated via FluentValidation before the handler executes, enforced through a MediatR Pipeline Behavior. Invalid requests never reach business logic.

### 8. AI is Infrastructure
All AI interactions (OpenAI embeddings, Claude/GPT for parsing and analysis) are abstracted behind interfaces in Application. The concrete implementations live in Infrastructure. Swapping providers requires zero Application changes.

### 9. Background Jobs are First-Class Citizens
Hangfire manages all background processing: job ingestion, AI enrichment, embedding generation. Every job is idempotent and retryable. Failed jobs are logged with full context.

### 10. Observability from Day One
Structured logging via Serilog on every significant operation. Every external API call is logged with duration, status, and source. Every background job logs start, completion, and failure with correlation IDs.

---

## Architecture Overview

```
src/
├── JobMarket.Domain/
│   ├── Entities/
│   ├── Enums/
│   ├── Exceptions/
│   └── Common/          ← BaseEntity, AuditableEntity
│
├── JobMarket.Application/
│   ├── Common/
│   │   ├── Interfaces/  ← IRepository<T>, IUnitOfWork, IAIService, IEmbeddingService
│   │   ├── Behaviors/   ← ValidationBehavior, LoggingBehavior
│   │   └── Mappings/    ← AutoMapper profiles
│   ├── Features/
│   │   ├── Jobs/
│   │   ├── CVs/
│   │   ├── Matching/
│   │   └── Analytics/
│   └── DTOs/
│
├── JobMarket.Infrastructure/
│   ├── Persistence/
│   │   ├── AppDbContext.cs
│   │   ├── Repositories/
│   │   └── UnitOfWork.cs
│   ├── Scrapers/        ← Strategy pattern per source
│   ├── ExternalAPIs/    ← Adzuna, Remotive clients
│   ├── AI/              ← OpenAI, Claude clients
│   ├── BackgroundJobs/  ← Hangfire job definitions
│   └── Caching/         ← Redis decorator
│
└── JobMarket.API/
    ├── Controllers/
    ├── Middleware/
    ├── Filters/
    └── Extensions/      ← DI registration
```

---

## Technology Stack

| Concern | Technology |
|---|---|
| Framework | ASP.NET Core (latest) |
| Language | C# (latest) |
| ORM | Entity Framework Core (latest) |
| Database | PostgreSQL (latest) |
| Vector Search | pgvector extension |
| CQRS / Mediator | MediatR (latest) |
| Validation | FluentValidation (latest) |
| Object Mapping | AutoMapper (latest) |
| Background Jobs | Hangfire (latest) |
| Caching | Redis via StackExchange.Redis (latest) |
| PDF Parsing | PdfPig (latest) |
| AI / Embeddings | OpenAI SDK (latest) |
| HTTP Clients | Typed HttpClient with Polly (latest) |
| Logging | Serilog (latest) |
| Containerization | Docker + Docker Compose |
| API Docs | Scalar / Swagger (latest) |

---

## Data Sources

| Source | Method | Coverage |
|---|---|---|
| Indeed | Web scraping | Global + Egypt |
| Glassdoor | Web scraping | Global |
| Wuzzuf | Web scraping | Egypt / MENA |
| Adzuna | Official API | Global |
| Remotive | Official API | Remote jobs |

---

## Non-Functional Requirements

### Performance
- Job search endpoint must respond in under 300ms for cached results
- Embedding generation is always async — never blocks HTTP response
- pgvector similarity search must handle 500K+ job records efficiently

### Scalability
- Scraping and AI enrichment are horizontally scalable via Hangfire distributed jobs
- Stateless API layer — scales behind a load balancer with no session state
- Redis cache isolates read load from the database

### Reliability
- All external HTTP calls use Polly retry with exponential backoff
- Every background job is idempotent — safe to retry on failure
- Scraper failures are isolated — one source failing does not affect others

### Security
- JWT authentication for all user-facing endpoints
- API key authentication for B2B consumers
- CV files validated for type and size before processing
- No raw CV text exposed in API responses after parsing

### Maintainability
- Every public interface has XML documentation
- No magic strings — all constants in dedicated static classes
- Feature folders keep related Commands, Queries, Handlers, and DTOs together

---

## Development Guidelines

### Naming Conventions
- Commands: `VerbNounCommand` (e.g. `UploadCvCommand`, `EnrichJobCommand`)
- Queries: `GetNounQuery` / `ListNounsQuery` (e.g. `GetJobMatchesQuery`)
- Handlers: `VerbNounCommandHandler` / `GetNounQueryHandler`
- DTOs: `NounDto` / `NounResponse` / `NounRequest`
- Interfaces: `IServiceName` (e.g. `IEmbeddingService`, `IScraperStrategy`)

### Branching Strategy
- `main` — production-ready only
- `develop` — integration branch
- `feature/feature-name` — individual features
- `fix/bug-description` — bug fixes

### Definition of Done
A feature is complete when:
- [ ] Handler logic implemented and tested
- [ ] Unit tests written for the handler
- [ ] FluentValidation rules defined
- [ ] AutoMapper profile added
- [ ] Controller endpoint wired
- [ ] Swagger documentation present
- [ ] No compiler warnings

---

## What This Project Is Not

- Not a job board — it does not host original job postings
- Not a social network — no user-to-user interaction
- Not a real-time system — near-real-time (minutes, not seconds) is acceptable for ingestion
