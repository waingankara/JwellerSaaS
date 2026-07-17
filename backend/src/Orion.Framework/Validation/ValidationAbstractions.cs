namespace Orion.Framework.Validation;

/// <summary>Validates framework requests without coupling to a validation library.</summary>
public interface IValidationPipeline
{
    /// <summary>Validates a model.</summary>
    Task<ValidationResult> ValidateAsync<T>(T model, CancellationToken cancellationToken);
}

/// <summary>Represents validation output.</summary>
public sealed record ValidationResult(IReadOnlyList<ValidationFailure> Failures)
{
    /// <summary>Gets whether validation succeeded.</summary>
    public bool IsValid => Failures.Count == 0;
}

/// <summary>Represents a validation failure.</summary>
public sealed record ValidationFailure(string PropertyName, string Message);
