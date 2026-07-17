# ADR-004: Multi Tenant

## Status

Accepted

## Decision

The Orion Framework distinguishes global masters, tenant masters, and transactional tables. Tenant context is resolved once per request and made available to framework pipelines for automatic tenant filtering.

## Consequences

Business modules should not manually implement tenant isolation for every master. The framework pipeline will apply tenant and permission filters consistently before Dapper executes parameterized SQL.
