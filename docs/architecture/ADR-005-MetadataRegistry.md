# ADR-005: Metadata Registry

## Status
Accepted

## Context
Orion Framework needs business-entity metadata without introducing Sprint 2 business modules.

## Decision
The framework provides `IMasterRegistry` and `MasterRegistry` for strongly typed registration of entity metadata. The registry delegates reflection to `IReflectionMetadataCache`, stores `MasterDefinition` instances by CLR type, and exposes generic registration such as `Register<T>()`.

## Consequences
Business modules can register their entities later without repository, service, or controller generation. Metadata remains strongly typed through definition records and attribute-driven discovery.
