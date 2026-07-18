# ADR-012-Tenant-Resolution

## Status
Accepted

## Context
Sprint 3 adds reusable Orion Framework identity, security, authorization, and tenant resolution capabilities for SaaS applications.

## Decision
Use domain identity and tenancy models in the domain layer, JWT bearer authentication, permission and role authorization services, header-first tenant resolution with a composite strategy, and Dapper-backed persistence abstractions.

## Consequences
The framework remains module-neutral while providing a consistent foundation for future business modules.
