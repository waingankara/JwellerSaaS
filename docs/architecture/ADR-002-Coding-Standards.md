# ADR-002: Coding Standards

## Status

Accepted

## Decision

All backend projects target .NET 9 with nullable reference types, implicit usings, file-scoped namespaces, XML documentation for public APIs, and warnings treated as errors. Dependencies are injected through constructors and asynchronous APIs accept cancellation tokens.

## Consequences

The solution favors explicit, maintainable code and catches quality issues during builds. Framework extension points must remain strongly typed and composable instead of relying on runtime magic or request-time reflection.
