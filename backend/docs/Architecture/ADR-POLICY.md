# ADR-POLICY.md

Version: 1.0

---

# Purpose

Architectural Decision Records (ADR) capture significant architectural decisions made during the evolution of the Orion Framework.

Every important technical decision must be documented before implementation.

The objective is to preserve architectural knowledge and explain WHY a decision was made rather than only WHAT was implemented.

---

# When is an ADR Required?

An ADR must be created whenever a change affects:

- Framework Architecture
- Public APIs
- Database Strategy
- Authentication
- Authorization
- Metadata Runtime
- SQL Generation
- Caching
- Event Architecture
- Messaging
- Performance Strategy
- Deployment Strategy
- Plugin Architecture

---

# Examples

Examples requiring ADRs

- Introduce Redis

- Replace RabbitMQ

- Introduce CQRS

- Support SQL Server

- Support Oracle

- Replace Dapper

- Change Metadata Discovery

- Introduce GraphQL

- Event Sourcing

- Plugin Marketplace

---

# Examples NOT requiring ADR

Bug Fixes

UI Changes

Business Rules

DTO Changes

Validation Rules

Additional Masters

Report Layout

Translations

---

# ADR Numbering

ADR-001

ADR-002

ADR-003

...

Never reuse numbers.

---

# ADR Template

Every ADR shall contain

Title

Status

Context

Decision

Consequences

Alternatives Considered

Implementation Notes

References

---

# Status

One of

Proposed

Accepted

Superseded

Deprecated

Rejected

---

# Review Process

Every ADR must be reviewed before merge.

Architecture impacting Pull Requests cannot be merged without an approved ADR.

---

# Ownership

The Lead Architect owns all ADR approvals.