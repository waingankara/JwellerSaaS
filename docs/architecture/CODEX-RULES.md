# Orion Codex Development Rules

## Rule 1 – Read Before Modifying

Before implementing any sprint:

1. Read ARCHITECTURE.md
2. Read CURRENT-STATE.md
3. Read the target sprint specification
4. Inspect the existing implementation
5. Identify conflicts before modifying code

## Rule 2 – Existing Code Wins

Do not replace working architecture merely because the sprint
specification describes an alternative implementation.

## Rule 3 – No Silent Architecture Changes

If implementation requires an architectural change:

STOP.

Explain the proposed change.

Create/update an ADR.

Do not silently redesign the framework.

## Rule 4 – Preserve Existing Contracts

Do not rename:

- public interfaces
- DTOs
- metadata contracts
- API contracts

unless explicitly required.

## Rule 5 – No Duplicate Types

Before creating a new class, search the repository for an existing
class with the same responsibility.

## Rule 6 – Reuse Existing Framework Components

Do not create:

CategoryRepository
CategoryController
CategoryCrudService

when generic Orion functionality already exists.

## Rule 7 – Build Before Completion

Run:

dotnet restore

dotnet build

dotnet test

before declaring the sprint complete.

## Rule 8 – Fix Compilation Errors

Do not leave compilation errors for the user to fix.

## Rule 9 – Do Not Undo Manual Fixes

Treat CURRENT-STATE.md and the latest approved commit as the
current implementation baseline.

## Rule 10 – Explain Deviations

At the end of every sprint report:

- Files changed
- Tests added
- Architecture changes
- Deviations from specification
- Potential risks