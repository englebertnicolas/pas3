
# PAS3 — Proof of Concept: idem PAS2, but mediator-less

### Summary
This repository is a small proof of concept (POC) demonstrating how to apply several architectural principles and patterns together. 
The difference with POC PAS2 is that we are **not using an in-memory mediator**.

Instead of introducing an in-memory mediator (such as MediatR, Wolverine), the project leverages ASP.NET Core Minimal APIs for direct routing. This modern approach eliminates unnecessary abstractions, simplifies dependency injection per endpoint, and significantly reduces boilerplate code while maintaining clean separation of concerns.

For the message broker part, this project uses Rebus (with RabbitMQ and the inbox/outbox patterns).
