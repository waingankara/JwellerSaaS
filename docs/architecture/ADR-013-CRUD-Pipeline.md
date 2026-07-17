# ADR-013: CRUD Pipeline

## Status
Accepted

## Context
Future Orion ERP modules need a reusable data access engine rather than one repository or service per business entity.

## Decision
Introduce `ICrudService<TEntity>` backed by a replaceable `ICrudPipeline`. The pipeline stages are authorization, validation, metadata resolution, tenant injection, audit injection, SQL building, Dapper execution, domain event publication, and API-ready response handling.

## Consequences
Each stage can be tested or replaced independently while business modules only register metadata and consume the generic service.
