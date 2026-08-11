# ADR-017: Framework Diagnostics

## Status
Accepted

## Context
Sprint 5 adds Orion Framework diagnostics so developers can inspect discovered metadata without redesigning the metadata-driven framework. Diagnostics must not expose internal framework details outside Development, while the application health endpoint remains available in every environment.

## Decision
Orion Framework exposes `IOrionDiagnosticsService` registered by `AddOrionFramework`. A hosted startup discovery service scans application/framework assemblies, builds metadata through the reflection cache, validates duplicate names/tables and required metadata, then registers definitions in `IMasterRegistry` before requests are accepted.

The JwellerSaaS API hosts `/api/diagnostics` and `/api/diagnostics/{entity}` as Development-only endpoints. Outside Development they return `404 Not Found`. `/api/health` remains unchanged and available outside Development.

## Consequences
Development diagnostics reuse existing Orion metadata components and avoid adding per-master controllers or repositories. The API owns environment gating for HTTP exposure, while the reusable framework owns diagnostics snapshot composition.
