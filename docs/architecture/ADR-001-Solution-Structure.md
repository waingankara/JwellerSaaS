# ADR-001: Solution Structure

## Status

Accepted

## Decision

The backend uses a Clean Architecture solution rooted at `backend/JwellerSaaS.sln` with API, Application, Domain, Infrastructure, Shared, Contracts, and Orion Framework projects.

## Consequences

The API hosts transport concerns only. Business modules register metadata in application or module assemblies while reusable framework capabilities remain inside Orion Framework. The structure avoids repositories and services per master so future ERP modules can be added without changing the framework foundation.
