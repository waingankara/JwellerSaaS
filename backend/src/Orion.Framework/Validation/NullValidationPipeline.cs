namespace Orion.Framework.Validation;

/// <summary>Default validation pipeline used when no validators are registered.</summary>
public sealed class NullValidationPipeline : IValidationPipeline
{
    /// <inheritdoc />
    public Task<ValidationResult> ValidateAsync<T>(T model, CancellationToken cancellationToken) => Task.FromResult(new ValidationResult(Array.Empty<ValidationFailure>()));
}
