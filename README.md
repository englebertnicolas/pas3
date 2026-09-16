
# PAS3 — Proof of Concept: Domain-centric Vertical Slice Architecture / CQRS / DDD

### Summary
Based on PAS2, this repository is a small proof of concept (POC) demonstrating how to apply several architectural principles and patterns together. 

Main changes between PAS2 and PAS3:

- **Pure VSA -> Domain-centric VSA**

  While pure VSA keeps everything inside a single project organized by folders, we extract Domain and Persistence into dedicated assemblies.
  This provides stronger discipline (prevents accidental leakage of infrastructure code into business logic) and centralization of the business logic.

- **Wolverine -> Rebus**

  We are using **Rebus** (instead of Wolverine) for the external messaging.

  The project leverages ASP.NET Core Minimal APIs for direct routing. This modern approach eliminates unnecessary abstractions, simplifies dependency injection per endpoint, and significantly reduces boilerplate code while maintaining clean separation of concerns.
  However, when a handler needs to be reused across multiple contexts, we rely on a custom, home-made in-memory mediator.

- **Read/write DbContexts**

  Read/write DbContext separation sample in project PAS.PolicyValuation.Persistence.
