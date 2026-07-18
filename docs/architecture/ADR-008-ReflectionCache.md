# ADR-008: Reflection Cache

## Status
Accepted

## Context
Metadata reflection is expensive and should not run repeatedly for the same entity type.

## Decision
`ReflectionMetadataCache` uses a thread-safe cache of lazily built `MasterDefinition` instances keyed by entity `Type`. Metadata construction occurs once per entity type and subsequent requests reuse the cached definition.

## Consequences
Runtime metadata access remains deterministic and efficient. Tests verify the same definition instance is returned for repeated lookups.
