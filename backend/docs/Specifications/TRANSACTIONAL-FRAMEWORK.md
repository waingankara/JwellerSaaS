# Orion Transactional Framework

## 1. Purpose

The Transactional Framework provides the reusable infrastructure required for
business transactions in JwellerSaaS.

Unlike Master Framework entities, transactional entities represent business
operations and workflows involving multiple entities, state transitions,
validation, inventory/accounting effects, and transactional consistency.

## 2. Architectural Principle

The framework owns:

- Transaction boundaries
- Database transaction management
- Transaction lifecycle
- Concurrency handling
- Validation pipeline
- Authorization integration
- Tenant isolation
- Audit handling
- Transactional event publishing
- Error handling
- Consistent transaction execution

Business modules own:

- Business rules
- Transaction-specific workflows
- Domain decisions
- State transitions
- Business validations
- Business-specific calculations

Generic CRUD must NOT be implemented inside transactional business modules.

## 3. Transaction Structure

A transaction may contain:

- Transaction Header
- Transaction Details
- Related entities
- State
- Business validations
- Database changes
- Audit information
- Domain events

All changes belonging to a single business operation must execute within
one database transaction when atomicity is required.

## 4. Transaction Lifecycle

The standard lifecycle is:

Request
→ Authorization
→ Tenant Resolution
→ Validation
→ Begin Database Transaction
→ Execute Business Operation
→ Persist Changes
→ Publish Transactional Events
→ Commit
→ Response

If any required operation fails:

Execute Rollback
→ Return standardized error

## 5. Transaction Boundary

The Transactional Framework must provide a reusable transaction abstraction.

Business services must not directly manage database connections or transaction
commit/rollback logic.

The framework owns the transaction boundary.

## 6. Concurrency

Transactional operations must support optimistic concurrency where required.

Concurrency conflicts must produce a standardized framework-level error.

## 7. Tenant Isolation

Every transactional operation must execute within the resolved tenant context.

Business code must not bypass tenant isolation.

## 8. Audit

Transactional operations must support:

- CreatedBy
- CreatedDate
- ModifiedBy
- ModifiedDate
- DeletedBy
- DeletedDate

Where applicable.

## 9. Events

Business operations may produce transactional/domain events.

Events must not be published before the database transaction reaches the
appropriate commit point.

## 10. Error Handling

Transactional failures must use the existing Orion Framework error-handling
standards.

Business exceptions must remain distinguishable from infrastructure failures.

## 11. Performance

The framework must avoid unnecessary:

- Reflection
- Database round trips
- Entity loading
- Serialization
- Transaction nesting

Metadata and reusable framework components should be cached where appropriate.

## 12. Initial Transactional Module

The first transactional module will be:

**Product**

The implementation will establish the reusable transactional pattern that
subsequent modules can follow.

Future modules include:

- Inventory
- Purchase
- Sales
- Repair
- Order
- RD Scheme
- Accounts