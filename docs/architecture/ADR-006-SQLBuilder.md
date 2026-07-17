# ADR-006: SQL Builder

## Status
Accepted

## Context
Reusable framework data access requires consistent SQL generation and parameterization.

## Decision
Orion Framework centralizes SQL creation in dedicated builders for insert, update, delete, select, search, pagination, existence, count, and duplicate checks. Builders validate SQL identifiers and emit parameterized statements.

## Consequences
Application code must use builders instead of ad hoc SQL concatenation. Query values are carried as parameters for Dapper execution.
