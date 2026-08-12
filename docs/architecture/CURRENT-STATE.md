# Orion Framework – Current State

Version: Sprint 4.5 Baseline

## Status

Sprint 4.5 is implemented and merged.

The repository contains the currently approved implementation.

## Important Rule

The current source code is the source of truth.

Sprint specifications describe intended architecture, but they must not
override working implementation without explicit approval.

## Framework Components

- Metadata Registry
- Metadata Discovery
- Generic Master Controller
- Generic CRUD Service
- CRUD Pipeline
- SQL Builder Factory
- Dapper PostgreSQL integration
- Tenant Context
- Current User Context
- Authorization
- Audit
- Soft Delete
- Search
- Pagination
- Generic API Response

## Current Known Fixes

Document every manual correction made after Sprint 4.5 here.

## Architectural Constraints

Do not introduce duplicate types.

Do not introduce entity-specific repositories.

Do not introduce entity-specific CRUD controllers.

Do not bypass the CRUD pipeline.

Do not change existing framework contracts without an ADR.

## Baseline

All changes in this document represent the approved
Sprint 4.5 baseline.