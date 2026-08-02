# Sprint 05
# Orion Framework Diagnostics & Validation

Version: 1.0

Status: Approved

---

# Objective

Before implementing business modules, Orion Framework shall provide
complete diagnostics, startup validation and runtime inspection tools.

The framework must be capable of validating itself.

Developers should never need to attach a debugger merely to determine
why an entity was not discovered or why metadata is missing.

---

# Background

Current framework functionality exists but diagnosing issues such as

- Relation Category is not known
- Metadata not registered
- Invalid entity configuration
- Missing audit columns
- Missing tenant columns

requires debugging source code.

The framework shall become self-diagnosing.

---

# Goals

Implement

- Metadata Diagnostics
- Runtime Diagnostics
- Startup Validation
- Framework Health
- Metadata Explorer
- SQL Preview
- Pipeline Tracing

No business modules.

---

# Deliverables

## 1. Metadata Explorer

Create a development-only endpoint

GET /api/framework/metadata

Returns

- Registered entities
- Table names
- Primary keys
- Tenant columns
- Audit columns
- Search columns
- Sort columns
- Metadata version

---

GET /api/framework/metadata/category

Returns complete metadata for Category.

---

## 2. Startup Validation

During application startup validate

- Duplicate entity names
- Duplicate table names
- Missing primary key
- Missing tenant column
- Missing audit columns
- Invalid metadata
- Duplicate routes

Startup must fail with descriptive exceptions.

---

## 3. Metadata Scanner Report

During startup log

Scanning Assemblies...

Assembly Orion.Domain

Assembly Jewellery.Domain

Discovered Entity Category

Discovered Entity Vendor

Metadata Registration Complete

---

## 4. Runtime Pipeline Trace

Development only.

Ability to trace

Request

↓

Authorization

↓

Validation

↓

Metadata

↓

SQL Builder

↓

Dapper

↓

Response

---

## 5. SQL Preview

Development endpoint

POST

/api/framework/sql-preview

Input

Entity

Operation

Request

Returns

Generated SQL

Parameters

No execution.

---

## 6. Framework Health

Endpoint

GET /api/framework/health

Returns

Metadata Status

Database

Redis

RabbitMQ

Storage

Configuration

---

## 7. Metadata Validation Tests

Integration tests

MetadataRegistry.Contains()

MetadataRegistry.Get()

Duplicate metadata

Invalid metadata

Startup failures

---

## 8. Diagnostic Logging

Every startup shall log

Framework Version

Metadata Version

Entity Count

Route Count

Permission Count

Startup Duration

---

## 9. Developer Dashboard

Development only.

Swagger section

Framework

Metadata

Health

Diagnostics

SQL Preview

---

## 10. Acceptance Criteria

Framework can diagnose

Unknown Entity

Missing Metadata

Invalid Metadata

Duplicate Metadata

Missing Primary Key

Missing Tenant

Missing Audit

without attaching a debugger.

---

## Definition of Done

Framework diagnostics implemented.

Startup validation implemented.

Metadata explorer implemented.

SQL preview implemented.

Health endpoint implemented.

Integration tests pass.

Zero compiler warnings.

Zero analyzer warnings.

Architecture review approved.

---

# Git

Branch

feature/sprint-5-framework-diagnostics

Commit

feat: add framework diagnostics and startup validation

Pull Request

Sprint 5 - Framework Diagnostics