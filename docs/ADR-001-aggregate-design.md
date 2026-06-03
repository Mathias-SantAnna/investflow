# ADR-001: Portfolio as Aggregate Root, Asset as Internal Entity

**Date:** 2026-06-03
**Status:** Accepted

## Context

The Portfolio Service tracks user investment portfolios. Each portfolio contains zero or more assets (e.g., stocks, ETFs, crypto). We need to decide how to model these in the Domain layer using DDD building blocks.

## Decision

`Portfolio` is the **aggregate root**. `Asset` is an **entity owned by Portfolio** — it has its own identity (`Id`) but is only ever accessed through the parent `Portfolio`. External code cannot fetch or mutate an `Asset` directly; all asset-level operations (`AddAsset`, `RemoveAsset`, `IncreaseAssetPosition`) go through methods on `Portfolio`.

`Money` is a **value object** used by `Asset.AverageCost` — immutable, compared by value, validated at construction.

## Rationale

1. **Transactional consistency boundary.** When a user adds an asset, the operation must atomically update the portfolio's asset list AND validate portfolio-level rules (e.g., max number of assets, currency consistency across the portfolio). Treating `Portfolio` as the root means the entire change set is one transaction, one repository save, one optimistic-concurrency check.

2. **Repository scope.** There is one `IPortfolioRepository` — not separate `IAssetRepository` and `IPortfolioRepository`. This prevents external code from loading an orphan `Asset` and modifying it outside the context of its owning portfolio, which would bypass invariants.

3. **Invariants live with the aggregate.** Rules like "a portfolio cannot hold duplicate symbols" or "total asset count ≤ 100" require knowing the full asset list — only the aggregate root has that view.

4. **Asset still needs identity** because the same symbol may be held, fully sold, then re-bought as a different position. The `Asset.Id` lets us distinguish lifecycle events even when symbol/quantity overlap.

## Consequences

**Positive:**
- Single repository simplifies infrastructure code and EF Core configuration.
- Domain rules are co-located on `Portfolio`, making the aggregate easy to reason about and test.
- Concurrency control is straightforward — version the aggregate root, not individual assets.

**Negative:**
- Large portfolios load all assets into memory on every operation. Acceptable here because realistic portfolio sizes are small (tens to low hundreds of assets); if this changes, we'd revisit by lazy-loading or splitting into sub-aggregates.
- Cannot expose `Asset` directly via the API as a top-level resource; all asset operations are namespaced under `/portfolios/{id}/assets/...`. This is also the desired REST design, so the constraint aligns with the public contract.

## Alternatives Considered

- **Asset as its own aggregate** with portfolio-asset link table: rejected because it would scatter portfolio-level invariants across two aggregates and require distributed transactions or eventual consistency for what should be a single atomic operation.
- **Money as a primitive `decimal Amount` + `string Currency`**: rejected because every consumer would re-implement validation, equality, and currency-mismatch detection. The value object pattern centralizes these rules and makes them testable in isolation.
