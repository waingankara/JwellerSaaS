# ADR-007: Tenant Engine

## Status
Accepted

## Context
Multi-tenant queries must consistently apply tenant scope when an entity exposes tenant metadata.

## Decision
The tenant engine reads `TenantContext` from `ITenantContextAccessor` and appends tenant predicates only when the current context is tenant scoped and the registered entity has a detected `TenantId` column.

## Consequences
Framework queries can opt into automatic tenant filtering without business-module code. Global scope remains available for trusted infrastructure operations.
