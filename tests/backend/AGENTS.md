# Backend test conventions

- Framework: xUnit (`[Fact]` / `[Theory]`, `TestContext`, `IAsyncLifetime`).
- Tests mirror the backend structure: one test project per bounded context (`PAS.<Context>.Tests`) + `Common/Testing` for shared infrastructure. Domain tests go in `<Context>/Domain/...`, feature (integration) tests in `<Context>/Features/...`.
- Shared base classes in `PAS.Testing`:
  - `IntegrationTestBase(IAppFixture)` — boots the real API via an `AppFixture`, resets the database before every test (`fixture.ResetDatabaseAsync()`), exposes an authenticated `HttpClient`. Inherit from a context-specific `AppFixture` (e.g. `Features/AppFixture.cs`) that registers `TestAuthenticationHandler`.
  - `DomainTestBase` — base for pure domain tests with an EF in-memory context (see `PolicyValuationContextFactory`).
- New integration tests: inherit the context's `AppFixture`-based base, use `Client` for HTTP calls, keep one use case per test class named after the feature (e.g. `CreateFundTests`).
- Run: `dotnet test` at repo root, or `dotnet test tests/backend/<Context>`.
- Do not mock DbContexts; integration tests run against a real (test) database via the fixture.
