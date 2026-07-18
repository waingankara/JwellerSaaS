# ADR-014: Query Engine

## Status
Accepted

## Context
ERP screens require dynamic filters, sorting, paging, search, projection, distinct queries, and top-N selection.

## Decision
Represent queries with `QueryDefinition`, `FilterDefinition`, `SortDefinition`, `QueryOptions`, and `SearchOptions`. SQL is generated with parameterized Dapper parameters and metadata-validated columns.

## Consequences
Future modules share one query surface while avoiding raw column names from callers.
