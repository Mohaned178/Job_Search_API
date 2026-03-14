# Job Market Intelligence API

A production-grade **Clean Architecture** REST API built with .NET 10 that provides AI-powered job market analysis, CV parsing, ATS scoring, semantic job matching via pgvector, and market trend analytics.

## Architecture Overview

```
JobMarket.Domain          — Entities, Value Objects, Exceptions
JobMarket.Application     — CQRS Handlers, Validators, Interfaces
JobMarket.Infrastructure  — EF Core, AI Services, Scrapers, Auth
JobMarket.API             — Controllers, Middleware, Program.cs
```

## Technology Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 10 |
| ORM | EF Core 10 + Npgsql |
| Vector DB | PostgreSQL 16 + pgvector |
| AI | OpenAI (text-embedding-3-small, gpt-4o-mini) |
| Cache | Redis (StackExchange.Redis) |
| Background Jobs | Hangfire (PostgreSQL backend) |
| Auth | JWT Bearer (HS256) + Refresh Tokens |
| Logging | Serilog |
| Docs | Scalar / OpenAPI |
| Testing | xUnit + Moq + FluentAssertions |

---

## How to Run Locally with Docker

### Prerequisites
- Docker Desktop (with Linux containers)
- An OpenAI API key

### 1. Clone and configure environment

```bash
git clone <repo-url>
cd "Job Market API"
cp .env.example .env
```

Edit `.env` and fill in:
```
POSTGRES_PASSWORD=your_secure_password
JWT_SECRET_KEY=your-jwt-secret-key-at-least-32-chars
OPENAI_API_KEY=sk-your-openai-api-key
```

### 2. Start all services

```bash
docker compose up --build
```

This will:
1. Build the API image from the multi-stage `Dockerfile`
2. Start PostgreSQL 16 with pgvector extension
3. Start Redis
4. Run any pending EF Core database migrations automatically on startup
5. Register recurring Hangfire jobs (daily job ingestion)

### 3. Access the API

| URL | Description |
|---|---|
| `http://localhost:8080/scalar` | Interactive API documentation (Scalar) |
| `http://localhost:8080/openapi/v1.json` | OpenAPI spec |
| `http://localhost:8080/hangfire` | Hangfire job dashboard (localhost only) |

### 4. Run for local development (without Docker)

Ensure PostgreSQL and Redis are running locally, then:

```bash
cd src/JobMarket.API
dotnet run
```

Or apply migrations manually:
```bash
dotnet ef database update --project src/JobMarket.Infrastructure --startup-project src/JobMarket.API
```

---

## Environment Variables

| Variable | Required | Default | Description |
|---|---|---|---|
| `ConnectionStrings__Postgres` | ✅ | — | PostgreSQL connection string |
| `ConnectionStrings__Redis` | ✅ | — | Redis connection string |
| `Jwt__SecretKey` | ✅ | — | HMAC-SHA256 signing key (≥32 chars) |
| `Jwt__Issuer` | ❌ | `JobMarketAPI` | JWT issuer claim |
| `Jwt__Audience` | ❌ | `JobMarketClient` | JWT audience claim |
| `Jwt__AccessTokenExpiryMinutes` | ❌ | `15` | Access token lifetime |
| `Jwt__RefreshTokenExpiryDays` | ❌ | `7` | Refresh token lifetime |
| `OpenAi__ApiKey` | ✅ | — | OpenAI API key |
| `OpenAi__EmbeddingModel` | ❌ | `text-embedding-3-small` | Embedding model |
| `OpenAi__ChatModel` | ❌ | `gpt-4o-mini` | Chat model for CV parsing & enrichment |
| `Scrapers__AdzunaAppId` | ❌ | — | Adzuna API application ID |
| `Scrapers__AdzunaApiKey` | ❌ | — | Adzuna API key |
| `AllowedOrigins__0` | ❌ | `http://localhost:3000` | CORS allowed origin(s) |
| `POSTGRES_PASSWORD` | ✅ (Docker) | — | PostgreSQL superuser password |

---

## API Endpoints

### Authentication — `/api/auth`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | Public | Register a new user account |
| POST | `/api/auth/login` | Public | Login and receive JWT token pair |
| POST | `/api/auth/refresh` | Public | Exchange refresh token for new token pair |

### Jobs — `/api/jobs`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/jobs` | Public | Search jobs with filters (keyword, location, country, seniority, type, salary) |
| GET | `/api/jobs/{id}` | Public | Get full job details |
| GET | `/api/jobs/{id}/related` | Public | Semantic similarity — top K related jobs |
| GET | `/api/jobs/trending` | Public | Recent trending jobs by recency |

### CV — `/api/cvs`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/cvs` | 🔒 JWT | Upload CV file (PDF or DOCX, max 5 MB) |
| GET | `/api/cvs/{id}/status` | 🔒 JWT | CV processing status |
| GET | `/api/cvs/{id}/ats-score` | 🔒 JWT | ATS score breakdown and suggestions |
| GET | `/api/cvs/{id}/matches` | 🔒 JWT | Top K semantically matched jobs |
| GET | `/api/cvs/{id}/gap-analysis?targetRole=` | 🔒 JWT | Skill gap analysis vs. a target role |
| GET | `/api/cvs/{id}/roadmap?targetRole=` | 🔒 JWT | Learning roadmap to close skill gaps |
| DELETE | `/api/cvs/{id}` | 🔒 JWT | Delete CV and storage file |

### Analytics — `/api/analytics`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/analytics/trending-skills?limit=20` | Public | Top N trending skills by market demand |
| GET | `/api/analytics/salary?jobTitle=&country=` | Public | Salary intelligence for a role/location |

### Preferences — `/api/preferences`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| PUT | `/api/preferences` | 🔒 JWT | Update job preference settings |
| GET | `/api/preferences/feed` | 🔒 JWT | Personalized job feed based on preferences |

### Watchlist — `/api/watchlist`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/watchlist` | 🔒 JWT | Get all watchlisted jobs |
| POST | `/api/watchlist/{jobId}` | 🔒 JWT | Add job to watchlist |
| DELETE | `/api/watchlist/{jobId}` | 🔒 JWT | Remove job from watchlist |

### Alerts — `/api/alerts`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/alerts` | 🔒 JWT | Get all job alerts |
| POST | `/api/alerts` | 🔒 JWT | Create a new keyword-based alert |
| DELETE | `/api/alerts/{id}` | 🔒 JWT | Delete an alert |

### Admin — `/api/admin` *(Admin role required)*

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/admin/ingest` | 🔒 Admin | Manually trigger job ingestion pipeline |

---

## Running Tests

```bash
dotnet test tests/JobMarket.Application.Tests/JobMarket.Application.Tests.csproj
```

**48 tests** pass: domain entity tests, handler tests (Register, Login, UploadCv), and validator tests (Register, UploadCv, SearchJobs).

---

## Background Jobs (Hangfire)

| Job | Schedule | Description |
|---|---|---|
| `job-ingestion-daily` | Daily (midnight) | Scrapes Wuzzuf, Indeed, Adzuna, Remotive for new jobs |
| CV processing | On upload | Extract text → Parse with AI → Embed → ATS score |
| Job enrichment | After ingestion | Enrich pending jobs with skills, seniority, salary via AI |
