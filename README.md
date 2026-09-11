
# PAS3 — Proof of Concept: idem PAS2, but with Rebus instead of Wolverine

### Summary
This repository is a small proof of concept (POC) demonstrating how to apply several architectural principles and patterns together. 
The main difference with POC PAS2 is that we are using **Rebus** (instead of Wolverine) for the external messaging.

The project leverages ASP.NET Core Minimal APIs for direct routing. This modern approach eliminates unnecessary abstractions, simplifies dependency injection per endpoint, and significantly reduces boilerplate code while maintaining clean separation of concerns.
However, when a handler needs to be reused across multiple contexts, we rely on a custom, home-made in-memory mediator to centralize and share the logic.
