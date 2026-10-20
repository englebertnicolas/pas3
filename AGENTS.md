# PAS3 — Agent Guide

Read this first. Directory-specific guides: `src/backend/AGENTS.md`, `src/frontend/AGENTS.md`, `tests/backend/AGENTS.md` (loaded automatically when working in those trees).

## What PAS3 is

PAS3 (Policy Administration System) is a proof of concept for a liability management system of a specialized life insurer focused on unit-linked (UL) products. It is the successor of PAS2 and demonstrates Domain-centric Vertical Slice Architecture + CQRS + DDD on .NET.

### Business domain in one paragraph

Policyholders hold unit-linked contracts backed by investment funds. The system tracks daily-fluctuating fund NAVs and FX rates, manages the policy lifecycle (premiums, surrenders, endorsements), and computes liabilities: reserves, valuations, retroactive change rollbacks, and projections. Regulatory outputs (solvency, embedded value) are future concerns. Use exact insurance terminology in code (Policy, Valuation, Endorsement, Reserve, NAV).

## Repository map

```
src/backend/
  Host/               Aspire AppHost — orchestration entry point (Keycloak, SQL Server, RabbitMQ containers, Azure KeyVault)
  Bff/Api             Backend-for-frontend (YARP reverse proxy, Keycloak authentication)
  PolicyAdmin/        Bounded context: policy lifecycle (Api / Domain / Persistence / Contracts / Client)
  PolicyValuation/    Bounded context: valuations, reserves, calculation layer (Api / Domain / Persistence with Read/ + Write/ split)
  MarketData/         Bounded context: funds, NAVs, currencies, FX rates (Api / Domain / Persistence / Contracts / Client)
  Messaging/Api       Messaging service
  Common/             Shared building blocks (Core, Domain, EntityFramework, Hosting, Mediator, OperationResults, Rebus, AspNetCore*, Guard, OpenApi, AspireServiceDefaults)
  DbMigrator/         Console app applying EF migrations for all DbContexts
  VaultSeeder/        Console app seeding Azure KeyVault secrets
src/frontend/         Placeholder (backoffice-app, broker-app, client-app, shared — currently empty)
tests/backend/        xUnit test projects mirroring the backend structure (+ Common/Testing shared fixtures)
scripts/CleanAll.cmd  Deletes all bin/obj/.vs/node_modules folders
```

## Commands

```bash
dotnet build PAS3.slnx
dotnet test
# Run the full local stack (Aspire orchestration, containers, service defaults):
dotnet run --project src/backend/Host/PAS.AppHost.csproj
# Backend tests for one context only:
dotnet test tests/backend/PolicyValuation
```

Visual Studio: solution file is `PAS3.slnx`. Set background analysis scope to "Entire solution".

## Global rules (apply everywhere)

1. **Domain-centric Vertical Slice Architecture.** Code is organized by feature/use case, not by technical layers. Each slice owns its endpoint, handler, validator, and models. Never introduce a Repository layer or generic `IRepository<T>` — use EF Core `DbContext` directly inside the slice.
2. **Bounded contexts.** Never mix domains (PolicyAdmin, PolicyValuation, MarketData) or create cross-domain dependencies. Cross-context communication goes through integration events (Rebus) or generated API clients, never direct DbContext/project references.
   **Accepted exception — MarketData read-only views:** PolicyValuation's `ValuationReadDbContext` reads immutable market reference data (funds, NAVs, currencies, FX rates) through read-only SQL **views** published by MarketData, instead of replicating it via event synchronization. Rules: SELECT-only access (no writes to the MarketData schema), query the views never the raw tables, no cross-schema foreign keys. Do not extend this pattern to mutable domain data — that still goes through integration events or API clients.
3. **CQRS.** Commands mutate state and enforce business rules; queries return projections/DTOs without side effects. In PolicyValuation the split is physical: separate Read and Write DbContexts.
4. **Code style.** `.editorconfig` is authoritative: apply it for every created or modified file (naming, `var`, expression-bodied preferences, file layout). Files are UTF-8, CRLF, final newline.
5. **Ubiquitous language.** Names use insurance/domain vocabulary, matching the existing code (e.g. `RollForwardValuation`, `ValuationLedgerAggregate`, `FundNav`).
6. **Client and Contracts projects** (`*/Client`, Kiota-generated; `*/Contracts`) are integration surfaces: never reference Persistence from them, and do not hand-edit generated code.
