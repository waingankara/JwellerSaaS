# ADR-015: Domain Events

## Status
Accepted

## Context
The framework must raise lifecycle events without choosing RabbitMQ or another transport.

## Decision
Add `IDomainEvent`, `IDomainEventPublisher`, lifecycle event names, and a no-op default publisher. Integrations can replace the publisher.

## Consequences
Entity lifecycle hooks are available now and transport decisions remain deferred.
