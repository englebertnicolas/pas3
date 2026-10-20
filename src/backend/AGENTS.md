# Backend conventions (.NET 10)

## Bounded context layout

Each bounded context (PolicyAdmin, PolicyValuation, MarketData) is a folder of sibling projects with fixed roles:

```
<Context>/
  Api/           ASP.NET Core Minimal API host: Program.cs, Features/, options, composition
  Domain/        Aggregates, entities, value objects, domain events, domain services — no infrastructure dependencies
  Persistence/    EF Core DbContext(s), entity configurations, migrations, read models
  Contracts/     Integration events shared with other contexts (referenced by consumers, never the reverse)
  Client/         Kiota-generated HTTP client for the context's API — do not hand-edit
```

PolicyValuation additionally splits Persistence into `Read/` (ValuationReadDbContext + read models/configurations) and `Write/` (ValuationDbContext + migrations). The Write DbContext owns aggregates; the Read DbContext serves queries.

## Feature structure (Vertical Slice)

```
Api/Features/<DomainArea>/<Feature>.cs          one file per simple use case (endpoint + handler + validator)
Api/Features/<DomainArea>/<Feature>/            complex use cases get a folder: Command, Endpoint, Handler, collaborators
```

Examples: `Features/Funds/UpsertFundNav.cs`, `Features/Policies/Valuations/RollForwardValuation/` (Command/Endpoint/Handler/Worker/Cache/Loader).

Naming: `Get*` / `Search*` for queries, `Create*` / `Upsert*` / `Issue*` for commands, `Handle*DomainEvent` / `Handle*IntegrationEvent` for handlers. All live in the Api project of the context, grouped by domain area.

## Composition (Program.cs)

Everything is wired in Program.cs with extension methods on `WebApplicationBuilder`/`IServiceCollection` from `PAS.*` Common projects: `.AddAspireServiceDefaults()`, `.AddMediator(thisAssembly)`, `.AddDefaultOpenApi()`, `.AddDomainEventHandlersFromAssembly()`, `.AddDomainEventDispatcher()`, `.AddDefaultRebus<TDbContext>(...)`, `.AddDbContext<T>(o => o.UseSqlServer(cnc))`. Secrets come from Azure KeyVault via `await builder.Configuration.AddVaultSecretsAsync()` plus local JSON files via `AddLocalJsonFiles`. Reuse an existing Common extension before inventing a new one; add cross-cutting concerns to `Common/`, not to a context.

## Common projects

| Project | Purpose |
|---|---|
| `PAS.Core` | Primitives (results, errors, abstractions) |
| `PAS.Domain` | Domain building blocks (base entity/aggregate, domain events) |
| `PAS.EntityFramework` | EF helpers: connection-string building, migrations host |
| `PAS.Hosting` | App host/pipeline helpers |
| `PAS.Mediator` | Home-made in-memory mediator (for handlers reused across endpoints) |
| `PAS.OperationResults` | Operation result types |
| `PAS.Rebus` | Rebus messaging wiring (RabbitMQ transport) |
| `PAS.AspNetCore*`, `PAS.OpenApi`, `PAS.Guard` | ASP.NET Core cross-cutting (auth Keycloak, diagnostics, endpoints, OpenAPI, guards) |
| `PAS.AspireServiceDefaults` | Aspire service defaults |

## Messaging

- **Domain events**: raised by aggregates, handled in the same context by `Handle*DomainEvent.cs` handlers (dispatched via `AddDomainEventDispatcher`).
- **Integration events**: defined in `Contracts/` (e.g. `FundNavUpdatedIntegrationEvent`), published/consumed over Rebus (RabbitMQ). Consumers are `Handle*IntegrationEvent.cs` in the consuming context's Api.

## EF Core

- No repository pattern. Handlers query `DbContext` directly.
- Entity configurations per aggregate in `Persistence/Configuration/` (and `Persistence/Read|Write/Configuration/` for PolicyValuation).
- Migrations live with their DbContext; names follow `V_<major>_<minor>_<patch>[_Seed]`. Migrations are applied by the `DbMigrator` console app for all contexts.
- Never edit an already-committed/applied migration or a `*ModelSnapshot.cs` — create a new migration with `dotnet ef migrations add` instead. A pending (not yet committed) migration may be adjusted by hand (custom SQL, seed data).
- A `*DesignTimeDbContext.cs` sits next to each DbContext for design-time tooling.

## Orchestration

`Host/AppHost.cs` (Aspire) is the local entry point: flags in `Infrastructure:*` config toggle Keycloak / SQL Server / RabbitMQ containers; `azurekeyvault` is a parameter. Keycloak realm import in `Host/Keycloak/realms.json`.
