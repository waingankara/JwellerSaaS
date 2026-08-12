# Sprint 05 – Generic Master Pipeline

**Version:** 1.0
**Status:** Approved
**Sprint:** 5
**Branch:** `feature/sprint-5-generic-master-pipeline`

---

# 1. Objective

Complete the Orion Framework Generic Master Pipeline so that `MasterApiController` is genuinely generic and is not coupled to the Category module.

The current implementation exposes:

```text
/api/master/{master}
```

but the controller is internally coupled to:

```text
ICategoryBusinessService
CategoryResponse
CreateCategoryRequest
UpdateCategoryRequest
CategorySearchRequest
CategoryDropdownResponse
```

and therefore only supports Category.

This sprint must remove that architectural limitation.

After Sprint 5, the same generic Master API must be capable of handling multiple master entities through metadata and generic framework services.

Category must become the first consumer of the generic Master pipeline rather than the implementation around which the pipeline is built.

---

# 2. Current Baseline

Sprint 4.5 is considered the current framework baseline.

The existing source code is the source of truth.

Do not blindly rewrite existing framework components.

Existing important components include:

```text
MasterApiController
ICategoryBusinessService
CategoryBusinessService
ICrudService<T>
ISqlExecutor
ITenantContextAccessor
MasterRegistry
MasterDefinition
IReflectionMetadataCache
QueryDefinition
FilterDefinition
SortDefinition
SQL Builders
```

The implementation must preserve working functionality unless a change is explicitly required by this specification.

---

# 3. Current Problem

The current controller has the following architecture:

```text
MasterApiController
        |
        v
ICategoryBusinessService
        |
        v
ICrudService<Category>
        |
        v
SQL Builder
        |
        v
PostgreSQL
```

The route is misleading:

```text
/api/master/{master}
```

because:

```csharp
if (!IsCategory(master))
    return NotFound();
```

prevents every master other than Category.

---

# 4. Target Architecture

The target architecture is:

```text
HTTP Request
     |
     v
MasterApiController
     |
     v
IMasterRegistry
     |
     v
MasterDefinition
     |
     v
Generic Master Pipeline
     |
     +--------------------+
     |                    |
     v                    v
Authorization       Business Rules
     |                    |
     +---------+----------+
               |
               v
         Generic CRUD
               |
               v
          SQL Builder
               |
               v
            Dapper
               |
               v
          PostgreSQL
```

The controller must not know whether the master is:

```text
Category
Vendor
Brand
Metal
Purity
Stone
Product
Customer
```

---

# 5. Architectural Principles

The following are mandatory.

## 5.1 Generic Controller

`MasterApiController` must not inject:

```text
ICategoryBusinessService
IVendorBusinessService
IBrandBusinessService
```

or any other entity-specific service.

---

## 5.2 Metadata Driven

The master name from the URL must be resolved through:

```text
IMasterRegistry
```

and ultimately:

```text
MasterDefinition
```

---

## 5.3 Automatic Discovery

Master entities must be discovered automatically during application startup.

Manual registration such as:

```csharp
registry.Register<Category>();
```

must not be required by the application.

---

## 5.4 Business Rules Remain Business-Specific

Category-specific rules must not move into the generic controller.

For example:

```text
Duplicate Category Code
Duplicate Category Name
System Category cannot be deleted
```

remain Category business rules.

The generic framework must provide an extension point through which those rules are invoked.

---

# 6. Master Registry

The existing `MasterRegistry` currently supports type-based lookup:

```csharp
Register<T>()
Get<T>()
```

Extend the registry to support name-based lookup.

Required capabilities:

```csharp
MasterDefinition Get(string masterName);

bool Contains(string masterName);

IReadOnlyCollection<MasterDefinition> GetAll();
```

The lookup must be case-insensitive.

These should all resolve the same master:

```text
category
Category
CATEGORY
```

---

# 7. Registry Internal Storage

The registry may maintain indexes such as:

```text
Type -> MasterDefinition

Normalized Entity Name -> MasterDefinition
```

The implementation may use `ConcurrentDictionary`.

Metadata must be immutable after application startup.

Runtime requests must not modify metadata.

---

# 8. Master Name

The framework must define a canonical master name.

For Category:

```text
category
```

The canonical name should be obtained from metadata rather than duplicated in the controller.

Do not write:

```csharp
if (master == "category")
```

inside the generic controller.

---

# 9. Automatic Metadata Discovery

At application startup:

```text
Scan configured assemblies
        |
        v
Find eligible master entities
        |
        v
Build MasterDefinition
        |
        v
Validate metadata
        |
        v
Register in MasterRegistry
        |
        v
Framework Ready
```

The process must occur once during startup.

Runtime requests must use cached metadata.

---

# 10. Discovery Rules

An entity qualifies as a master when it satisfies the existing Orion metadata conventions.

Do not introduce a second metadata model.

Reuse the existing:

```text
IReflectionMetadataCache
MasterDefinition
ColumnDefinition
SearchDefinition
DuplicateDefinition
AuditDefinition
PermissionDefinition
TenantDefinition
ValidationDefinition
```

If the existing framework uses an attribute/base type to identify masters, reuse it.

If the current implementation does not yet have a reliable marker, introduce one consistently and document it.

---

# 11. Startup Validation

During startup validate:

* Duplicate entity names
* Duplicate table names
* Missing primary key
* Invalid table name
* Invalid column configuration
* Invalid tenant configuration
* Invalid audit configuration
* Invalid search configuration
* Invalid permission configuration

Application startup must fail with a clear error if critical metadata is invalid.

---

# 12. Generic Master Service

Introduce a framework-level service responsible for executing generic master operations.

Suggested contract:

```csharp
IMasterCrudService
```

The exact implementation name may be adjusted if an equivalent existing abstraction already exists.

The service must operate using:

```text
masterName
MasterDefinition
operation
request data
```

It must not be hard-coded to Category.

---

# 13. Generic Operations

The generic pipeline must support:

```text
Create
GetById
Update
Delete
Search
Count
Exists
SoftDelete
Restore
Dropdown
```

---

# 14. Generic Controller

Refactor:

```text
MasterApiController
```

so that it depends only on framework-level abstractions.

It must not depend on:

```text
ICategoryBusinessService
CategoryBusinessService
Category DTOs
Category domain classes
```

The controller should resolve the master using the route:

```text
/api/master/{master}
```

---

# 15. Generic API

The following routes must be supported:

```text
POST   /api/master/{master}

GET    /api/master/{master}/{id}

PUT    /api/master/{master}/{id}

DELETE /api/master/{master}/{id}

POST   /api/master/{master}/search

GET    /api/master/{master}/count

GET    /api/master/{master}/{id}/exists

PATCH  /api/master/{master}/{id}/soft-delete

PATCH  /api/master/{master}/{id}/restore

GET    /api/master/{master}/dropdown
```

The exact HTTP verb/path may remain unchanged where already established by the current API contract.

Do not introduce unnecessary breaking API changes.

---

# 16. Request Model

A generic controller cannot use:

```csharp
CreateCategoryRequest
```

for every entity.

The generic pipeline should therefore accept a generic JSON representation internally.

Preferred approach:

```csharp
JsonElement
```

or an equivalent JSON abstraction already used by the project.

Do not use:

```text
Dictionary<string, object>
```

unless required by the existing framework.

The generic pipeline must map request properties according to metadata.

---

# 17. Response Model

The generic pipeline should return a generic response representation.

The implementation may use:

```text
object
JsonElement
Dictionary
```

or an existing framework response abstraction.

Do not create:

```text
CategoryResponse
VendorResponse
BrandResponse
```

inside the generic controller.

Business-specific response DTOs may continue to exist for specialized endpoints outside the generic Master API.

---

# 18. DTO Compatibility

Existing Category DTOs must not be unnecessarily deleted.

The objective is to separate:

```text
Generic Master API
```

from:

```text
Category-specific business contracts
```

If Category DTOs are no longer required by the generic API, they may remain available for future specialized APIs.

---

# 19. Business Rule Extension

Introduce or reuse a generic business rule abstraction.

Preferred conceptual contract:

```csharp
IBusinessRule<T>
```

or an equivalent framework abstraction.

The framework must be able to resolve business rules for an entity type.

Example:

```text
Category
    |
    v
CategoryBusinessRules

Vendor
    |
    v
VendorBusinessRules
```

The controller must not resolve these directly.

---

# 20. Category Business Rules

Preserve the current Category business behavior.

The following rules must continue to work:

```text
Category Code required

Category Name required

Code maximum length 20

Name maximum length 200

Display Order >= 0

Duplicate Category Code prevented

Duplicate Category Name prevented

System Category cannot be deleted

Restore duplicate protection
```

Do not weaken existing behavior during genericization.

---

# 21. Category Business Service

The existing:

```text
CategoryBusinessService
```

may be refactored internally to fit the generic business-rule mechanism.

Do not delete valid business rules merely to make the controller generic.

The desired end state is:

```text
Generic Controller
       |
       v
Generic Master Pipeline
       |
       v
Category Business Rules
```

not:

```text
Generic Controller
       |
       v
CategoryBusinessService
```

---

# 22. Generic Tenant Handling

Every generic operation must respect the existing tenant context.

Tenant information must come from:

```text
ITenantContextAccessor
```

or the existing Orion tenant abstraction.

The API client must not be allowed to override the tenant.

Tenant filtering must be applied automatically by the generic pipeline.

---

# 23. Generic Authorization

Authorization must be metadata-driven.

The framework must resolve permissions from:

```text
MasterDefinition.Permission
```

or the existing permission metadata mechanism.

Do not hard-code:

```text
CATEGORY_CREATE
CATEGORY_UPDATE
```

inside the generic controller.

---

# 24. Authorization DI Lifetime

The implementation must not introduce the existing DI error:

```text
Cannot consume scoped service
ICurrentUserAccessor
from singleton IAuthorizationHandler
```

Authorization handlers that depend on scoped request services must use an appropriate lifetime.

Verify all authorization registrations before completion.

---

# 25. SQL Generation

Generic CRUD must continue to use existing SQL builders.

No Category-specific SQL may be added to:

```text
MasterApiController
Generic Master Service
Metadata Registry
SQL Builder
```

The SQL builder must receive metadata.

---

# 26. Business-Specific SQL

Business-specific SQL may exist only where a business rule genuinely requires it and the generic query engine cannot express the rule.

Such SQL must remain inside the business/application layer.

The existing Category duplicate check may remain temporarily if necessary.

If the generic `DuplicateDefinition` can fully support the rule, migrate Category to that mechanism.

Do not redesign duplicate handling unless required.

---

# 27. FilterDefinition

There must be only one canonical `FilterDefinition` for generic query filtering.

Do not introduce duplicate definitions such as:

```text
Orion.Framework.Pagination.FilterDefinition
Orion.Framework.Query.FilterDefinition
```

The canonical query filtering model should remain under:

```text
Orion.Framework.Query
```

unless the existing architecture explicitly proves otherwise.

All affected files must use the canonical type.

---

# 28. Metadata Diagnostics

Add a development-only diagnostic capability.

The framework should expose metadata information such as:

```text
Entity Name
Entity Type
Table Name
Primary Key
Tenant Column
Audit Configuration
Search Configuration
Permission Configuration
```

Suggested endpoint:

```text
GET /api/framework/metadata
```

and:

```text
GET /api/framework/metadata/{master}
```

These endpoints must not expose sensitive information in Production.

---

# 29. Unknown Master Handling

Request:

```text
GET /api/master/unknown
```

must return a standardized error.

Do not return:

```text
500 Internal Server Error
```

for a normal unknown-master request.

Preferred status:

```text
404 Not Found
```

using the standard Orion API response format.

---

# 30. API Error Handling

Use the existing:

```text
ApiResponse<T>
```

or the established framework response abstraction.

Do not introduce a second response format.

---

# 31. Reflection Performance

Reflection is permitted during:

```text
Application Startup
```

Reflection must not be performed repeatedly during every HTTP request.

Runtime master resolution must use cached metadata.

---

# 32. Thread Safety

The registry must be thread-safe.

Multiple concurrent requests must safely resolve the same metadata.

Metadata must not be mutated after startup.

---

# 33. Backward Compatibility

Existing Category functionality must continue to work.

Existing endpoints must not be unnecessarily broken.

The following must remain functional:

```text
Create Category

Get Category

Update Category

Delete Category

Search Category

Count Category

Exists Category

Soft Delete Category

Restore Category

Dropdown Category
```

---

# 34. Testing Requirements

## 34.1 Registry Tests

Test:

```text
Register entity

Get by Type

Get by Name

Contains

GetAll

Case-insensitive lookup

Unknown entity

Duplicate entity
```

---

## 34.2 Metadata Discovery Tests

Verify:

```text
Category discovered automatically

Category registered automatically

No manual Category registration required
```

---

## 34.3 Controller Tests

Verify:

```text
/api/master/category
```

works.

If another test master is available, verify:

```text
/api/master/{otherMaster}
```

also resolves through the same controller.

Do not create a fake second master solely to satisfy the test unless necessary; prefer an existing valid master entity.

---

## 34.4 Category Regression Tests

Verify all current Category operations.

---

## 34.5 Tenant Tests

Create data under Tenant A.

Create data under Tenant B.

Verify:

```text
Tenant A cannot read Tenant B data.

Tenant B cannot read Tenant A data.
```

---

## 34.6 Authorization Tests

Verify permissions are respected.

---

## 34.7 Unknown Master Tests

Verify unknown master returns:

```text
404
```

and standardized response.

---

# 35. Logging

Startup should log:

```text
Orion Metadata Discovery Started

Entity discovered: Category

Metadata registered: category

Orion Metadata Discovery Completed

Registered Master Count: N
```

Do not log sensitive data.

---

# 36. Files Expected to Change

Codex must inspect the actual repository before deciding exact files.

Likely areas include:

```text
Orion.Framework.Metadata
Orion.Framework.Crud
Orion.Framework.Query
Orion.Framework.Authorization
JwellerSaaS.Api
JwellerSaaS.Application.Masters
JwellerSaaS.Tests
```

Do not modify unrelated modules.

---

# 37. Protected Architecture

Do not modify architecture unnecessarily in:

```text
Authentication
Tenant Context
Database Connection Infrastructure
Dapper Infrastructure
SQL Builder contracts
Existing API response contracts
```

If a protected component genuinely requires modification, report it before making a broad change.

---

# 38. Mandatory Build Verification

Before completion run:

```bash
dotnet restore
dotnet build
dotnet test
```

All projects must build successfully.

No compilation errors.

No new warnings.

All tests must pass.

---

# 39. Architecture Verification

Before completion verify:

```text
MasterApiController contains no Category dependency.

MasterApiController contains no IsCategory() method.

MasterApiController contains no Category DTO.

MasterRegistry supports lookup by master name.

Category can be resolved through metadata.

Metadata discovery is automatic.

Generic CRUD pipeline handles Category.

Category business rules remain outside the controller.

Tenant isolation remains active.

Authorization remains active.

SQL builders remain responsible for generic CRUD SQL.
```

---

# 40. Definition of Done

Sprint 5 is complete only when all of the following are true:

* [ ] MasterApiController is genuinely generic.
* [ ] No Category-specific dependency exists in MasterApiController.
* [ ] `IsCategory()` has been removed.
* [ ] MasterRegistry supports name-based lookup.
* [ ] MasterRegistry supports `Contains`.
* [ ] MasterRegistry supports retrieving all registered masters.
* [ ] Automatic master discovery works.
* [ ] Metadata is validated during startup.
* [ ] Metadata is cached.
* [ ] Generic CRUD works with Category.
* [ ] Category business rules remain functional.
* [ ] Tenant isolation works.
* [ ] Authorization works.
* [ ] Soft Delete works.
* [ ] Restore works.
* [ ] Search works.
* [ ] Count works.
* [ ] Exists works.
* [ ] Dropdown works.
* [ ] Unknown master returns 404.
* [ ] Development metadata diagnostics work.
* [ ] No duplicate `FilterDefinition` exists.
* [ ] Authorization DI lifetime issue is resolved.
* [ ] Unit tests pass.
* [ ] Integration tests pass.
* [ ] `dotnet build` succeeds.
* [ ] `dotnet test` succeeds.
* [ ] No new compiler warnings.
* [ ] No unrelated framework refactoring is introduced.

---

# 41. Codex Completion Report

Before declaring Sprint 5 complete, Codex must report:

```text
Files Changed:
...

Files Added:
...

Files Removed:
...

Tests Added:
...

Tests Passed:
...

Framework Changes:
...

Business Changes:
...

Architecture Changes:
...

Specification Deviations:
...

Known Risks:
...
```

If an architecture change was necessary, identify the affected ADR.

---

# 42. Final Architecture

The final architecture must be:

```text
                    HTTP
                     |
                     v
             MasterApiController
                     |
                     v
              IMasterRegistry
                     |
                     v
              MasterDefinition
                     |
                     v
          Generic Master Pipeline
                     |
          +----------+----------+
          |                     |
          v                     v
   Authorization          Business Rules
          |                     |
          +----------+----------+
                     |
                     v
                Generic CRUD
                     |
                     v
                SQL Builder
                     |
                     v
                  Dapper
                     |
                     v
                PostgreSQL
```

The Category module must be a consumer of this architecture, not a dependency of the controller.

---

# 43. Out of Scope

The following are explicitly out of scope:

```text
Vendor implementation

Brand implementation

Product implementation

Inventory implementation

Purchase implementation

Sales implementation

Frontend changes

Mobile application changes

RabbitMQ implementation

Redis implementation

Cloudflare R2 implementation

Workflow engine

Reporting engine
```

These will be addressed by future sprints.

---

# 44. Sprint Success Criteria

The most important success criterion is:

> A second master can be added without modifying `MasterApiController`.

If adding Vendor requires:

```text
VendorController
VendorApiController
VendorRepository
changes to MasterApiController
```

Sprint 5 has failed.

The desired development model is:

```text
Add Entity
     |
Add Metadata
     |
Add Business Rules
     |
Add Migration
     |
Done
```

while Orion continues to provide the generic infrastructure.

---

# End of Sprint 05 Specification
