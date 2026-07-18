namespace JwellerSaaS.Domain.Common;

/// <summary>Base identity contract for persisted entities.</summary>
public abstract class BaseEntity
{
    /// <summary>Gets or sets the database identity key.</summary>
    public long Id { get; set; }
}
