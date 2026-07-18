namespace Orion.Framework.DomainEvents;

/// <summary>Framework domain event abstraction.</summary>
public interface IDomainEvent
{
    /// <summary>Event UTC occurrence time.</summary>
    DateTimeOffset OccurredAt { get; }
}

/// <summary>Entity lifecycle event raised by the CRUD engine.</summary>
public sealed record EntityLifecycleEvent(Type EntityType, string Name, object? Entity, DateTimeOffset OccurredAt) : IDomainEvent;

/// <summary>Publishes domain events without binding to a transport.</summary>
public interface IDomainEventPublisher
{
    /// <summary>Publishes an event.</summary>
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);
}

/// <summary>No-op domain event publisher.</summary>
public sealed class NullDomainEventPublisher : IDomainEventPublisher
{
    /// <inheritdoc />
    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken) => Task.CompletedTask;
}

/// <summary>Lifecycle event names.</summary>
public static class DomainEventNames
{
    /// <summary>Entity is being created.</summary>
    public const string EntityCreating = nameof(EntityCreating);

    /// <summary>Entity was created.</summary>
    public const string EntityCreated = nameof(EntityCreated);

    /// <summary>Entity is being updated.</summary>
    public const string EntityUpdating = nameof(EntityUpdating);

    /// <summary>Entity was updated.</summary>
    public const string EntityUpdated = nameof(EntityUpdated);

    /// <summary>Entity is being deleted.</summary>
    public const string EntityDeleting = nameof(EntityDeleting);

    /// <summary>Entity was deleted.</summary>
    public const string EntityDeleted = nameof(EntityDeleted);

    /// <summary>Entity is being restored.</summary>
    public const string EntityRestoring = nameof(EntityRestoring);

    /// <summary>Entity was restored.</summary>
    public const string EntityRestored = nameof(EntityRestored);
}
