# Job Market Intelligence API — Specification

## Product Overview

Job Market Intelligence API is a backend service that aggregates job listings from multiple sources, enriches them with AI analysis, and provides intelligent career guidance. The core differentiator is the CV Intelligence layer — users upload their CV and receive job matching, ATS scoring, gap analysis, and a personalized learning roadmap based on real market data.

---

## Functional Requirements

### Module 1 — Job Aggregation

**FR-J01** The system shall ingest job listings from Indeed, Glassdoor, and Wuzzuf via web scraping.

**FR-J02** The system shall ingest job listings from Adzuna and Remotive via their official APIs.

**FR-J03** All ingested jobs shall be normalized into a unified schema regardless of source.

**FR-J04** The system shall deduplicate job listings based on title, company, and location fingerprint.

**FR-J05** Each job listing shall be enriched with: extracted skills, seniority level, salary range (if available), and a generated embedding vector.

**FR-J06** Background jobs shall run on a configurable schedule to fetch new listings and update existing ones.

**FR-J07** Each job listing shall track its source, ingestion timestamp, and enrichment status.

---

### Module 2 — Job Search & Discovery

**FR-S01** Users shall be able to search jobs by keyword with full-text search support.

**FR-S02** Users shall be able to filter jobs by: category, country, city, seniority level, job type (full-time, part-time, remote), and salary range.

**FR-S03** Users shall be able to combine multiple filters in a single request.

**FR-S04** Search results shall be paginated with configurable page size.

**FR-S05** The system shall return trending jobs based on listing volume over configurable time windows (1h, 6h, 24h, 7d).

**FR-S06** The system shall return related jobs for any given job listing using vector similarity.

**FR-S07** Users shall be able to retrieve a chronological timeline of listings for a major ongoing topic (e.g. layoffs, AI jobs).

---

### Module 3 — CV Intelligence

**FR-C01** Users shall be able to upload a CV in PDF or DOCX format (max 5MB).

**FR-C02** The system shall extract structured data from the CV: full name, contact info, skills, work experience, education, languages, and total years of experience.

**FR-C03** CV processing shall be asynchronous — the upload endpoint returns immediately with a CV ID and processing status.

**FR-C04** The system shall generate an ATS score (0–100) for the uploaded CV with detailed breakdown: keyword density, formatting quality, section completeness, and skill relevance.

**FR-C05** The system shall provide specific, actionable improvement suggestions for each ATS score dimension.

**FR-C06** The system shall generate a semantic embedding for the CV to enable vector-based job matching.

**FR-C07** Users shall be able to retrieve a ranked list of matching jobs for their CV, ordered by match score (0–100).

**FR-C08** Each job match result shall include the match score, matched skills, and missing skills.

**FR-C09** The system shall perform gap analysis: given the user's target role or seniority level, return the skills they are missing ranked by market demand frequency.

**FR-C10** The system shall generate a personalized learning roadmap based on gap analysis, ordered by priority derived from real job listing data.

**FR-C11** Users shall be able to have their CV summarized by AI on request.

**FR-C12** The system shall support re-processing an existing CV if the user uploads an updated version.

---

### Module 4 — Market Analytics

**FR-A01** The system shall expose an endpoint returning the top N most in-demand skills globally and by region.

**FR-A02** The system shall expose skill trend data showing demand change over time (week-over-week, month-over-month).

**FR-A03** The system shall provide salary intelligence: average, median, min, and max salary by role, seniority, and location.

**FR-A04** The system shall provide a market summary for any given job title: top hiring companies, required skills, average salary, and demand trend.

**FR-A05** The system shall expose source-level statistics: how many jobs each source contributes and their freshness.

---

### Module 5 — User Preferences & Personalization

**FR-P01** Authenticated users shall be able to set interest categories (e.g. backend, data science, devops).

**FR-P02** Authenticated users shall be able to set preferred locations and job types.

**FR-P03** The system shall return a personalized feed of jobs based on stored preferences.

**FR-P04** Authenticated users shall be able to create keyword alerts — receive notifications when new jobs matching their keywords are ingested.

**FR-P05** Authenticated users shall be able to save jobs to a personal watchlist.

---

### Module 6 — Authentication & Authorization

**FR-AU01** The system shall support user registration and login with JWT authentication.

**FR-AU02** JWT access tokens shall expire after 15 minutes; refresh tokens shall expire after 7 days.

**FR-AU03** B2B consumers shall authenticate via API keys with configurable rate limits per key.

**FR-AU04** CV data is private — users can only access their own CVs.

**FR-AU05** Admin endpoints (trigger scraping, view system stats) shall require an admin role.

---

## Non-Functional Requirements

**NFR-01 Response Time** — Search and filter endpoints must respond in under 500ms at p95 under normal load.

**NFR-02 Throughput** — The system must handle 100 concurrent users without degradation.

**NFR-03 Availability** — API uptime target of 99.5% excluding scheduled maintenance.

**NFR-04 Data Freshness** — Job listings must be refreshed at minimum every 6 hours.

**NFR-05 Scalability** — The ingestion pipeline must be horizontally scalable with no shared mutable state between workers.

**NFR-06 Security** — All endpoints must enforce authentication. CV files must be scanned for type spoofing. No PII exposed in logs.

**NFR-07 Observability** — Every request must be traceable via correlation ID. All background jobs must emit structured logs.

---

## User Stories

### Guest User

```
As a guest user,
I want to search for jobs by keyword and filter by location and category,
So that I can explore the job market without creating an account.
```

```
As a guest user,
I want to see trending skills in the tech market,
So that I can understand what skills are in demand right now.
```

```
As a guest user,
I want to see salary ranges for a given job title and location,
So that I can benchmark my expectations before applying.
```

---

### Registered Job Seeker

```
As a registered user,
I want to upload my CV and receive an ATS score with improvement tips,
So that I can increase my chances of passing automated screening systems.
```

```
As a registered user,
I want to receive a ranked list of jobs that match my CV,
So that I can focus my applications on the best-fit opportunities.
```

```
As a registered user,
I want to see exactly which skills I am missing for senior backend roles,
So that I know where to invest my learning time.
```

```
As a registered user,
I want to receive a prioritized learning roadmap based on my skill gaps,
So that I learn the skills that will have the highest impact on my job search first.
```

```
As a registered user,
I want to set my interests and preferred locations,
So that my job feed shows me relevant listings without manual filtering every time.
```

```
As a registered user,
I want to create a keyword alert for "remote .NET jobs",
So that I am notified immediately when matching jobs are added.
```

```
As a registered user,
I want to save interesting jobs to a watchlist,
So that I can revisit them later without searching again.
```

---

### B2B API Consumer

```
As a B2B developer,
I want to query job market data by skill and region via API key,
So that I can power career tools and dashboards with real market intelligence.
```

```
As a B2B developer,
I want to submit a CV text and receive a structured match result,
So that I can integrate CV analysis into my own recruitment platform.
```

---

### Admin

```
As an admin,
I want to manually trigger a scraping run for a specific source,
So that I can refresh data outside the scheduled window when needed.
```

```
As an admin,
I want to view ingestion statistics per source (jobs fetched, failed, deduplicated),
So that I can monitor the health of the data pipeline.
```

---

## API Endpoints Summary

### Jobs
```
GET    /api/jobs                          Search and filter jobs
GET    /api/jobs/{id}                     Get job by ID
GET    /api/jobs/{id}/related             Get related jobs via vector similarity
GET    /api/jobs/trending                 Get trending jobs
GET    /api/jobs/timeline?topic={topic}   Get chronological listing for a topic
```

### CV Intelligence
```
POST   /api/cv/upload                     Upload CV file
GET    /api/cv/{id}                       Get parsed CV data
GET    /api/cv/{id}/status                Get processing status
GET    /api/cv/{id}/ats-score             Get ATS score and suggestions
GET    /api/cv/{id}/matches               Get ranked job matches
GET    /api/cv/{id}/gap-analysis          Get skill gap analysis
GET    /api/cv/{id}/roadmap               Get personalized learning roadmap
POST   /api/cv/{id}/summarize             Request AI summary
DELETE /api/cv/{id}                       Delete CV and all associated data
```

### Analytics
```
GET    /api/analytics/skills/trending         Top trending skills
GET    /api/analytics/skills/{skill}/trend    Demand trend for a specific skill
GET    /api/analytics/salary                  Salary intelligence by role/location
GET    /api/analytics/market-summary          Market summary for a job title
GET    /api/analytics/sources                 Source health and statistics
```

### User Preferences
```
GET    /api/preferences                   Get user preferences
PUT    /api/preferences                   Update preferences
GET    /api/preferences/feed              Get personalized job feed
POST   /api/alerts                        Create keyword alert
GET    /api/alerts                        List user alerts
DELETE /api/alerts/{id}                   Delete alert
POST   /api/watchlist/{jobId}             Save job to watchlist
GET    /api/watchlist                     Get watchlist
DELETE /api/watchlist/{jobId}             Remove from watchlist
```

### Authentication
```
POST   /api/auth/register                 Register new user
POST   /api/auth/login                    Login and receive tokens
POST   /api/auth/refresh                  Refresh access token
POST   /api/auth/logout                   Revoke refresh token
```

### Admin
```
POST   /api/admin/scrape/{source}         Trigger manual scrape
GET    /api/admin/stats                   System statistics
GET    /api/admin/jobs/queue              View enrichment queue status
```

---

## Domain Entities (Summary)

| Entity | Key Fields |
|---|---|
| Job | Id, Title, Company, Location, Description, Skills[], SalaryMin, SalaryMax, SeniorityLevel, JobType, Source, SourceUrl, Embedding, EnrichmentStatus, PostedAt |
| CV | Id, UserId, RawText, ParsedData (JSON), Embedding, AtsScore, EnrichmentStatus, UploadedAt |
| User | Id, Email, PasswordHash, Role, RefreshToken, RefreshTokenExpiry |
| UserPreferences | UserId, Categories[], PreferredLocations[], JobType, SeniorityLevel |
| JobAlert | Id, UserId, Keywords[], IsActive, LastTriggeredAt |
| Watchlist | UserId, JobId, SavedAt |
| SkillTrend | Skill, Date, DemandCount, Source |

---

## Constraints & Assumptions

- Web scraping is performed responsibly with rate limiting and respect for robots.txt
- CV files are stored temporarily and deleted after processing unless the user is authenticated
- AI enrichment (embeddings, parsing) is always async and never blocks an HTTP response
- The system does not store or display full job descriptions from scraped sources — only metadata and extracted structured data
- Salary data is treated as a range estimate, not a guaranteed figure
