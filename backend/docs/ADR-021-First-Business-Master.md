# ADR-021: First Business Master

## Status
Accepted

## Context
Category is the first business master implemented on Orion Framework. It validates that an ERP master can be introduced by defining an entity, DTO contract, business validation service, database migration, and metadata registration without adding a dedicated controller, repository, or CRUD SQL implementation.

## Decision
Category is registered with `IMasterRegistry` during application startup. The entity carries Orion metadata attributes for the table, primary key, searchable fields, duplicate fields, dropdown fields, audit fields, soft-delete field, tenant field, and concurrency token. A metadata-driven master endpoint routes `/api/master/category` requests to the registered master behavior.

## Adding New Business Masters
1. Create the database table and indexes, including tenant, audit, soft-delete, and row-version columns.
2. Add a domain entity marked with `MasterAttribute` and column metadata attributes.
3. Add request/response DTOs for create, update, search, response, and dropdown use cases.
4. Add a business service only for business validation, duplicate checks, domain rules, and future domain events.
5. Register the entity in the metadata registry.
6. Do not add a repository or hand-written CRUD SQL.

## Why No Controller or Repository Is Required
Orion Framework owns generic CRUD, search, pagination, sorting, soft-delete, restore, audit, and tenant metadata behavior. Category is exposed through the metadata-aware master API route rather than a `CategoryController`. Data access uses the framework `ICrudService<Category>` and `ISqlExecutor` only for business duplicate checks.

## Metadata Registration
`MetadataRegistration.RegisterBusinessMasters` registers `Category` with `IMasterRegistry`. The registry reflects the entity attributes once and provides table, columns, search, duplicate, tenant, audit, validation, and permission metadata to framework services.

## Business Service Responsibilities
`CategoryBusinessService` contains business-only rules: required code/name, maximum lengths, tenant-scoped uniqueness, display order validation, duplicate prevention during restore, and system-category delete protection. It does not generate CRUD SQL, use Dapper directly, or depend on HTTP abstractions.

## Consequences
Future masters can follow Category as the reference implementation: metadata attributes plus registration are sufficient for framework exposure, while the business service remains focused on domain rules.
