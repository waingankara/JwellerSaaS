# ADR-003: Database Conventions

## Status

Accepted

## Decision

PostgreSQL is the system of record. Database identifiers use `snake_case`, identity keys use `BIGINT`, and all data access uses Dapper with parameterized SQL through Npgsql connections.

## Consequences

The framework will provide SQL building and execution primitives rather than Entity Framework, generic repositories, repository-per-entity patterns, or dynamic SQL concatenation. Tables include audit columns for creation, modification, soft deletion, and row versioning.
