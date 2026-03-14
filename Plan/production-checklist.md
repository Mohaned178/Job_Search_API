# Job Market Intelligence API — Production Readiness Checklist

## How to Use This Document

Go through every section before considering the project production-ready.
Every item marked `[CRITICAL]` is a hard blocker — do not deploy without it.
Items marked `[IMPORTANT]` should be resolved within the first week of deployment.
Items marked `[NICE]` are enhancements that improve resilience over time.

**Sign-off rule**: Every `[CRITICAL]` item must be checked before a production deployment is approved.

---

## 1. Codebase & Build

- [ ] `[CRITICAL]` Project builds with zero errors and zero warnings (`dotnet build --configuration Release`)
- [ ] `[CRITICAL]` All `[MUST]` tasks in `tasks.md` are completed and verified
- [ ] `[CRITICAL]` No hardcoded secrets, API keys, passwords, or connection strings anywhere in the codebase
- [ ] `[CRITICAL]` No `TODO`, `FIXME`, or `throw new NotImplementedException()` in any code path reachable at runtime
- [ ] `[CRITICAL]` `.gitignore` excludes `appsettings.Development.json`, `.env`, `*.user`, `bin/`, `obj/`
- [ ] `[IMPORTANT]` No unused using statements, unused variables, or dead code
- [ ] `[IMPORTANT]` All compiler warnings treated — either resolved or explicitly suppressed with justification
- [ ] `[NICE]` Static analysis tool configured (e.g. SonarAnalyzer or Roslynator) with no critical issues

---

## 2. Configuration & Secrets Management

- [ ] `[CRITICAL]` All secrets loaded from environment variables or a secrets manager — never from `appsettings.json` in production
- [ ] `[CRITICAL]` `appsettings.Production.json` exists and references only environment variable placeholders — no real values
- [ ] `[CRITICAL]` The following environment variables are defined and verified in the production environment:
  - `ConnectionStrings__DefaultConnection`
  - `Jwt__SecretKey`
  - `Jwt__Issuer`
  - `Jwt__Audience`
  - `OpenAI__ApiKey`
  - `Redis__ConnectionString`
  - `Scrapers__UserAgent`
  - `ASPNETCORE_ENVIRONMENT=Production`
- [ ] `[CRITICAL]` JWT secret key is at minimum 256 bits (32 characters) — not a simple word or phrase
- [ ] `[CRITICAL]` Database password is strong and not reused from any other service
- [ ] `[IMPORTANT]` Secrets are rotatable without a code deployment — only environment variable change required
- [ ] `[IMPORTANT]` No secrets appear in application logs at any log level
- [ ] `[NICE]` Secrets managed via a dedicated vault (e.g. AWS Secrets Manager, Azure Key Vault, HashiCorp Vault)

---

## 3. Database

- [ ] `[CRITICAL]` All EF Core migrations applied to the production database
- [ ] `[CRITICAL]` `pgvector` extension installed on the production PostgreSQL instance (`CREATE EXTENSION IF NOT EXISTS vector`)
- [ ] `[CRITICAL]` IVFFlat index on `jobs.embedding` is created and verified
- [ ] `[CRITICAL]` Unique index on `(source, source_job_id)` in the `jobs` table is active — prevents duplicate ingestion
- [ ] `[CRITICAL]` Database user used by the application has minimum required permissions — no superuser, no `DROP` permissions
- [ ] `[CRITICAL]` Automated database backups are configured and tested — restore drill performed at least once
- [ ] `[IMPORTANT]` Connection pool size tuned for expected load (`Max Pool Size` in connection string)
- [ ] `[IMPORTANT]` Long-running query timeout set to prevent runaway queries locking the database
- [ ] `[IMPORTANT]` `pg_stat_statements` extension enabled for query performance monitoring
- [ ] `[IMPORTANT]` All foreign key constraints verified — no orphaned records in any table
- [ ] `[NICE]` Read replica configured for analytics and search queries to offload the primary

---

## 4. Security

### Authentication & Authorization
- [ ] `[CRITICAL]` All endpoints except `/api/auth/register` and `/api/auth/login` require authentication
- [ ] `[CRITICAL]` JWT tokens are validated: signature, expiry, issuer, and audience
- [ ] `[CRITICAL]` Refresh tokens are stored hashed in the database — not in plain text
- [ ] `[CRITICAL]` Admin endpoints protected by role-based authorization (`[Authorize(Roles = "Admin")]`)
- [ ] `[CRITICAL]` Users can only access their own CVs — every handler verifies `cv.UserId == requestingUserId`
- [ ] `[CRITICAL]` B2B API key authentication validated on every request — invalid keys return 401

### Input Validation & Injection
- [ ] `[CRITICAL]` FluentValidation active on every command and query via the MediatR pipeline behavior
- [ ] `[CRITICAL]` File upload validates MIME type and file extension — not just extension alone (prevent spoofing)
- [ ] `[CRITICAL]` File upload rejects files larger than 5MB before reading the stream
- [ ] `[CRITICAL]` All database queries go through EF Core parameterized queries — no raw string concatenation in SQL
- [ ] `[CRITICAL]` Raw SQL used in vector search uses parameterized inputs — not string interpolation

### Transport & Headers
- [ ] `[CRITICAL]` HTTPS enforced — HTTP redirects to HTTPS in production (`app.UseHttpsRedirection()`)
- [ ] `[CRITICAL]` HSTS header configured (`app.UseHsts()`)
- [ ] `[IMPORTANT]` Security headers present on all responses:
  - `X-Content-Type-Options: nosniff`
  - `X-Frame-Options: DENY`
  - `X-XSS-Protection: 1; mode=block`
  - `Referrer-Policy: no-referrer`
- [ ] `[IMPORTANT]` CORS policy restricts allowed origins — not `AllowAnyOrigin()` in production
- [ ] `[IMPORTANT]` Rate limiting configured per user / per IP to prevent abuse of AI-powered endpoints

### CV & PII
- [ ] `[CRITICAL]` Raw CV text is never returned in any API response after parsing
- [ ] `[CRITICAL]` CV files stored in a private location — not publicly accessible via direct URL
- [ ] `[CRITICAL]` Deleted CVs are permanently removed from both database and file storage
- [ ] `[IMPORTANT]` PII (email, phone) from parsed CVs excluded from all log output

---

## 5. API & Contracts

- [ ] `[CRITICAL]` All endpoints return consistent error response format: `{ "message": "...", "errors": [] }`
- [ ] `[CRITICAL]` HTTP status codes are semantically correct:
  - `200` for successful GET
  - `201` for successful resource creation
  - `202` for accepted async operations (CV upload)
  - `400` for validation errors
  - `401` for unauthenticated requests
  - `403` for unauthorized (authenticated but wrong role or ownership)
  - `404` for not found
  - `500` for unexpected server errors
- [ ] `[CRITICAL]` `ExceptionHandlingMiddleware` catches all unhandled exceptions — no raw stack traces exposed to clients
- [ ] `[IMPORTANT]` All list endpoints are paginated — no endpoint returns an unbounded result set
- [ ] `[IMPORTANT]` Swagger / Scalar UI disabled in production
- [ ] `[IMPORTANT]` API versioning implemented (`/api/v1/...`)
- [ ] `[NICE]` Response compression enabled for large JSON payloads

---

## 6. Background Jobs & Ingestion Pipeline

- [ ] `[CRITICAL]` Hangfire server is running and processing the job queue
- [ ] `[CRITICAL]` `JobIngestionJob` recurring schedule registered and active (every 6 hours minimum)
- [ ] `[CRITICAL]` All background jobs are idempotent — safe to run multiple times without duplicating data
- [ ] `[CRITICAL]` Failed job retry policy configured (3 retries with exponential backoff)
- [ ] `[CRITICAL]` Hangfire dashboard is secured — not publicly accessible without admin authentication
- [ ] `[IMPORTANT]` At least one full ingestion cycle completed and verified — jobs appear in the database
- [ ] `[IMPORTANT]` CV processing job tested end-to-end: upload → parse → embed → ATS score → Enriched status
- [ ] `[IMPORTANT]` Job enrichment queue is processing — no growing backlog of Pending jobs
- [ ] `[IMPORTANT]` Failed job alerting configured — someone is notified on repeated failures
- [ ] `[NICE]` Circuit breaker per scraper source — if a source fails 5 consecutive times it pauses for 1 hour

---

## 7. Performance

- [ ] `[CRITICAL]` pgvector IVFFlat index built with `ANALYZE` run after bulk insert (`ANALYZE jobs`)
- [ ] `[CRITICAL]` No N+1 query issues — EF Core queries use `.Include()` where needed, lazy loading disabled
- [ ] `[IMPORTANT]` Search endpoint measured — p95 response time under 500ms under normal load
- [ ] `[IMPORTANT]` CV upload and processing tested with a real 2MB PDF — completes within 60 seconds
- [ ] `[IMPORTANT]` Redis caching verified for search endpoints — cache hit reduces database load
- [ ] `[IMPORTANT]` Embedding generation never called synchronously on an HTTP request — always via background job
- [ ] `[IMPORTANT]` Database connection pool not exhausted under simulated concurrent load
- [ ] `[NICE]` Load test performed (minimum 50 concurrent users) using k6 or NBomber
- [ ] `[NICE]` Slow query log enabled in PostgreSQL — queries over 200ms are flagged

---

## 8. Observability & Logging

- [ ] `[CRITICAL]` Serilog configured for production with structured JSON output
- [ ] `[CRITICAL]` Log level in production is `Information` — `Debug` and `Trace` are off
- [ ] `[CRITICAL]` Every HTTP request logs: method, path, status code, duration, and correlation ID
- [ ] `[CRITICAL]` Every background job logs: job name, start time, outcome, and duration
- [ ] `[CRITICAL]` Every external API call logs: target URL, HTTP status, and duration
- [ ] `[CRITICAL]` Application startup log confirms `ASPNETCORE_ENVIRONMENT=Production`
- [ ] `[IMPORTANT]` Correlation ID propagated through all log entries within a single request
- [ ] `[IMPORTANT]` Logs shipped to a centralized sink (Seq, Datadog, ELK, CloudWatch, or equivalent)
- [ ] `[IMPORTANT]` Error-level log triggers an alert notification (email or Slack)
- [ ] `[IMPORTANT]` Hangfire failure logs include job type, input, and full exception with stack trace
- [ ] `[NICE]` Distributed tracing configured via OpenTelemetry for end-to-end request visibility

---

## 9. Health Checks

- [ ] `[CRITICAL]` `/health` endpoint returns `200 OK` when all dependencies are reachable
- [ ] `[CRITICAL]` Health check verifies: PostgreSQL connectivity, Redis connectivity, Hangfire server status
- [ ] `[IMPORTANT]` `/health/live` returns `200` if the process is alive — used for container liveness probe
- [ ] `[IMPORTANT]` `/health/ready` returns `200` only when all dependencies are healthy — used for readiness probe
- [ ] `[IMPORTANT]` Health check endpoints are excluded from authentication requirements
- [ ] `[NICE]` Health check UI dashboard available at a secured internal path

---

## 10. Docker & Infrastructure

- [ ] `[CRITICAL]` Production `Dockerfile` uses multi-stage build — final image is runtime-only, no SDK included
- [ ] `[CRITICAL]` Docker image runs as a non-root user
- [ ] `[CRITICAL]` `docker compose up` starts all services cleanly from a fresh state
- [ ] `[CRITICAL]` Database migrations run automatically on container startup — no manual step required
- [ ] `[CRITICAL]` All sensitive values passed as environment variables — nothing sensitive baked into the image
- [ ] `[IMPORTANT]` Docker image size is reasonable — under 300MB for the API image
- [ ] `[IMPORTANT]` Container restart policy set to `unless-stopped` or `always`
- [ ] `[IMPORTANT]` PostgreSQL data volume persisted — `docker compose down` does not wipe data
- [ ] `[IMPORTANT]` Redis persistence configured (`appendonly yes` or RDB snapshots) — survives container restarts
- [ ] `[IMPORTANT]` Resource limits set on containers (CPU and memory) — a runaway Hangfire job cannot starve the API
- [ ] `[NICE]` Docker image tagged with Git commit SHA — every deployment traceable to a specific commit
- [ ] `[NICE]` Container registry configured for image storage and versioning

---

## 11. Testing Verification

- [ ] `[CRITICAL]` All unit tests pass (`dotnet test --configuration Release`)
- [ ] `[CRITICAL]` No test skipped without a documented reason
- [ ] `[IMPORTANT]` Test coverage for Application handlers above 70%
- [ ] `[IMPORTANT]` All validator tests pass — every rule has at least one valid and one invalid test case
- [ ] `[IMPORTANT]` CV upload → process → match flow tested manually end-to-end with a real CV file
- [ ] `[IMPORTANT]` Job ingestion tested with at least two sources returning real data
- [ ] `[IMPORTANT]` Full authentication flow tested: register → login → call protected endpoint → refresh → logout
- [ ] `[NICE]` Integration tests run against a real PostgreSQL instance via Testcontainers

---

## 12. External Services Verification

- [ ] `[CRITICAL]` OpenAI API key is valid and has sufficient quota for expected production load
- [ ] `[CRITICAL]` Adzuna API credentials verified — test request returns job listings successfully
- [ ] `[CRITICAL]` Remotive API verified — test request returns job listings successfully
- [ ] `[IMPORTANT]` Scraper rate limiting configured — minimum 2–5 second delay between requests per source
- [ ] `[IMPORTANT]` Polly retry policies tested — API responds correctly when an external service is temporarily down
- [ ] `[IMPORTANT]` OpenAI rate limit handling tested — system degrades gracefully when quota is hit (jobs retry, no crash)
- [ ] `[NICE]` Fallback behavior defined when OpenAI is unavailable — enrichment pauses gracefully rather than failing loudly

---

## 13. Final Smoke Test

Run these requests manually in order against the production URL immediately after deployment.
All 13 steps must pass before the deployment is considered successful.

```
# 1. Health check
GET /health
→ expect 200, all dependency checks green

# 2. Register a new user
POST /api/auth/register
Body: { "email": "test@example.com", "password": "Test@12345" }
→ expect 201

# 3. Login
POST /api/auth/login
Body: { "email": "test@example.com", "password": "Test@12345" }
→ expect 200 with accessToken and refreshToken

# 4. Search jobs (guest — no token)
GET /api/jobs?keyword=backend&country=egypt
→ expect 200 with paginated job results

# 5. Upload a real CV (with Bearer token)
POST /api/cv/upload
Content-Type: multipart/form-data, file: real PDF under 5MB
→ expect 202 with cvId and status "Pending"

# 6. Poll CV status until Enriched
GET /api/cv/{cvId}/status
→ expect EnrichmentStatus "Enriched" within 60 seconds

# 7. Get ATS score
GET /api/cv/{cvId}/ats-score
→ expect score 0–100 with breakdown object and suggestions list

# 8. Get job matches
GET /api/cv/{cvId}/matches
→ expect ranked list with matchScore per job and matched/missing skills

# 9. Get gap analysis
GET /api/cv/{cvId}/gap-analysis?target=senior-backend
→ expect list of missing skills with market demand percentages

# 10. Get learning roadmap
GET /api/cv/{cvId}/roadmap
→ expect ordered skill list with priority justification from market data

# 11. Get trending skills
GET /api/analytics/skills/trending
→ expect list of skills with demand counts and trend direction

# 12. Trigger manual scrape (Admin token required)
POST /api/admin/scrape/wuzzuf
→ expect 202, verify job count in database increases after a few minutes

# 13. Confirm Hangfire dashboard is secured
GET /hangfire  (without admin credentials)
→ expect 401 or redirect to login — must NOT return the dashboard
```

---

## 14. Go-Live Sign-Off

- [ ] Every `[CRITICAL]` item in sections 1–12 is checked
- [ ] All 13 smoke test steps passed on the production URL
- [ ] At least one other person reviewed the deployment configuration
- [ ] Rollback plan documented — know exactly how to revert to the previous version in under 10 minutes
- [ ] On-call contact defined — someone reachable for the first 24 hours after go-live
- [ ] Monitoring dashboard open and showing green metrics during and after deployment
- [ ] Team notified that the system is live
