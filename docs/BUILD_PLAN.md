# InvestFlow — Build Plan

A staged delivery plan focused on **test coverage, code quality, and demonstrable craftsmanship** rather than architectural breadth. Each phase is roughly one week at 15–20 hours.

> **Why this is shorter than typical "microservices portfolio" plans:** an earlier draft of this plan covered 8 phases including a price microservice, RabbitMQ messaging, Azure Functions, and multi-cloud deployment. That scope was trimmed in favor of **finishing one well-tested REST API and deploying it**, which is more honest signal for a portfolio reviewer than a sprawling unfinished system. The dropped scope is listed at the bottom — it can return as separate projects if relevant.

---

## Phase 1 — Domain layer + unit tests _(in progress)_

**Goal:** A pure C# domain model with ≥ 90% test coverage. No infrastructure dependencies, no framework references.

**Done:**
- ✅ `Money` value object — immutable, `IEquatable<Money>`, validating constructor, currency normalization
- ✅ `Asset` entity — encapsulated mutable state, weighted-average cost recomputation, currency-mismatch protection
- ✅ 18+ xUnit tests covering happy paths and rejection invariants

**To do:**
- `Portfolio` aggregate root — owns the asset list; enforces "no duplicate symbols", currency consistency, max-asset count
- Domain events: `AssetAddedEvent`, `AssetRemovedEvent`
- `IPortfolioRepository` interface — Domain owns the contract; Infrastructure implements it
- Tests for every aggregate behavior and every rejection path

---

## Phase 2 — Application layer + use case tests

**Goal:** Use cases expressed as commands and queries, validated and tested in isolation from the API and database.

- `CreatePortfolioCommand` + handler + tests
- `AddAssetCommand` + handler + tests
- `RemoveAssetCommand` + handler + tests
- `GetPortfolioQuery` + handler + tests
- DTOs for the API boundary (no domain types leak into responses)
- FluentValidation rules with dedicated validator tests

**Pattern:** handler tests use a mocked `IPortfolioRepository`. The repository contract is exercised separately in integration tests (Phase 3).

---

## Phase 3 — Infrastructure + REST API + integration tests

**Goal:** A working REST API with a proper test pyramid.

- EF Core `AppDbContext` and `PortfolioRepository` implementation
- SQLite for local development (zero install — just a single file)
- `Portfolio.API` controllers wrapping the application-layer handlers
- Global exception-handling middleware that maps domain exceptions to appropriate HTTP status codes
- Swagger/OpenAPI documentation
- **Integration tests using `WebApplicationFactory<Program>`** — spins up the API in-memory with an in-memory database, hits real endpoints with `HttpClient`, and asserts on JSON responses. This is the test layer most junior candidates skip and is the highest-signal differentiator.
- Health-check endpoint at `/health`

---

## Phase 4 — CI/CD + coverage + deploy

**Goal:** Make the test discipline visible to reviewers and the API publicly accessible.

- GitHub Actions workflow: `build → test → publish coverage report`
- Coverlet for code coverage; coverage percentage badge in README
- Deploy `Portfolio.API` to Azure App Service free tier
- Live API URL added to README
- CI gates pushes to `main` — broken or failing tests block merge

---

## Phase 5 — Frontend _(optional, only if Phases 1–4 ship on time)_

A small React + TypeScript SPA consuming the API. Adds "full-stack-enough" coverage but **not the priority** — the back-end test discipline is the story.

- Vite + React + TypeScript scaffold
- One page: portfolio list + detail view
- React Query for server state and caching
- A handful of Vitest tests for the components

---

## Out of scope _(intentionally)_

These appeared in earlier plans but were dropped to focus on shipping one polished, tested, deployed thing:

- ~~Separate Price microservice~~
- ~~RabbitMQ / event-driven communication~~
- ~~MongoDB for price history~~
- ~~Azure Functions / serverless Alert service~~
- ~~Kubernetes manifests~~
- ~~Multi-cloud deployment (Azure + AWS Lambda)~~
- ~~Centralized observability (Seq / OpenTelemetry)~~

For a junior or QA-leaning portfolio piece the right bar is **finished, tested, deployed**, not **architecturally exhaustive**. Each item above could be a follow-up project if a target role specifically asks for it.
