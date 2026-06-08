# InvestFlow

> A test-driven REST API for managing investment portfolios.
> Built with .NET 10 and a strong focus on test coverage, clear boundaries, and verifiable correctness.

[![Build](https://github.com/Mathias-SantAnna/investflow/actions/workflows/ci.yml/badge.svg)](https://github.com/Mathias-SantAnna/investflow/actions)
![Coverage](https://img.shields.io/badge/coverage-pending-lightgrey)
![.NET](https://img.shields.io/badge/.NET-10-purple)

**Live API:** _Coming soon (Azure App Service free tier — Phase 4)._

---

## Why this project

InvestFlow exists to demonstrate the discipline I bring to software quality:

- **TDD-first** — every domain class has unit tests written alongside it, not bolted on later.
- **Layered test pyramid** — unit tests for the domain, integration tests for the API.
- **Documented design decisions** — each non-obvious architectural choice is justified in an [ADR](docs/).
- **Continuous validation** — automated tests run on every push via GitHub Actions.

It's the kind of codebase I'd want to inherit: clear boundaries, predictable behavior, easy to change without breaking.

---

## Test strategy

| Layer | What it tests | Tool |
|---|---|---|
| **Unit** | Domain entities and value objects in isolation. No infrastructure. | xUnit |
| **Use case** _(Phase 2)_ | Application command/query handlers with mocked repositories. | xUnit + Moq |
| **Integration** _(Phase 3)_ | REST endpoints end-to-end against an in-memory database. | xUnit + `WebApplicationFactory` |

**Coverage targets:** Domain ≥ 90%, Application ≥ 80%, API ≥ 70%.

Run all tests locally with `dotnet test`. Coverage reports via [coverlet](https://github.com/coverlet-coverage/coverlet) (added in Phase 4).

---

## Tech stack

- **.NET 10** / **C# 14**
- **ASP.NET Core Web API** for HTTP endpoints
- **Entity Framework Core** for persistence (SQLite in dev, SQL Server in production)
- **xUnit** for testing
- **coverlet** for code coverage
- **Docker** for containerized local development
- **GitHub Actions** for CI

---

## Run locally

Requires the .NET 10 SDK and Git.

```bash
git clone https://github.com/Mathias-SantAnna/investflow.git
cd investflow
dotnet build
dotnet test
dotnet run --project Portfolio.API
```

Then visit `https://localhost:5001/swagger` for the API explorer (available once Phase 3 lands).

---

## Project layout

```
InvestFlow/
├── Portfolio.Domain/          Pure C# — entities, value objects, no dependencies
├── Portfolio.Application/     Use cases (commands, queries, validation)
├── Portfolio.Infrastructure/  EF Core context, repository implementations
├── Portfolio.API/             REST endpoints, middleware, Swagger
├── Portfolio.Tests/           Unit + integration tests
├── InvestFlow.ApiGateway/     Edge routing (kept minimal — single-service for now)
└── docs/                      ADRs and the build plan
```

---

## Status

**Phase 1 — Domain layer:** in progress.

| Item | Status | Tests |
|---|---|---|
| `Money` value object | ✅ done | 11 passing |
| `Asset` entity | ✅ done | 7 passing (theories expand to ~11 cases) |
| `Portfolio` aggregate root | ⏳ next | — |
| `IPortfolioRepository` interface | ⏳ next | — |

See [`docs/BUILD_PLAN.md`](docs/BUILD_PLAN.md) for the full roadmap.

---

## Design decisions

Architectural choices are recorded as ADRs (Architecture Decision Records):
- [ADR-001: Portfolio as Aggregate Root](docs/ADR-001-aggregate-design.md)

---

## License

This project is open source for portfolio review. Code is yours to read, fork, and learn from.
