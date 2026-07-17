# ADR-016: Bulk Operations

## Status
Accepted

## Context
ERP imports and maintenance actions need batched create, update, delete, soft-delete, and restore operations.

## Decision
Expose bulk methods on `ICrudService<TEntity>` with caller-selected batch size. The initial implementation reuses the same CRUD pipeline per item so validation, tenant, audit, diagnostics, and events remain consistent.

## Consequences
Bulk semantics are available immediately and can later be optimized with provider-specific multi-row SQL without changing module code.
