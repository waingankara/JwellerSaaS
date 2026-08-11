# ADR-017: Framework Diagnostics

## Status
Accepted

## Context
Sprint 5 adds Orion Framework diagnostics so developers can inspect registered metadata without redesigning the metadata-driven framework. Diagnostics must not expose internal framework details outside Development, while the application health endpoint remains available in every environment.

## Decision
Orion Framework exposes an `IOrionDiagnosticsService` registered by `AddOrionFramework`. The service reads registered master metadata through `IMasterRegistry.GetAll()` and returns a serializable snapshot for API hosting layers.

The JwellerSaaS API hosts `/api/diagnostics` as a Development-only endpoint. Outside Development it returns `404 Not Found`. `/api/health` remains unchanged and available outside Development.

## Consequences
Development diagnostics reuse existing Orion metadata components and avoid adding per-master controllers or repositories. The API owns environment gating for HTTP exposure, while the reusable framework owns diagnostics snapshot composition.
