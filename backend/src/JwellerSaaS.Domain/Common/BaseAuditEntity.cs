namespace JwellerSaaS.Domain.Common;

/// <summary>Base entity with audit and soft-delete columns.</summary>
public abstract class BaseAuditEntity : BaseEntity
{
    /// <summary>Gets or sets the user that created the row.</summary>
    public long CreatedBy { get; set; }
    /// <summary>Gets or sets when the row was created.</summary>
    public DateTimeOffset CreatedDate { get; set; }
    /// <summary>Gets or sets the user that last modified the row.</summary>
    public long? ModifiedBy { get; set; }
    /// <summary>Gets or sets when the row was last modified.</summary>
    public DateTimeOffset? ModifiedDate { get; set; }
    /// <summary>Gets or sets the user that deleted the row.</summary>
    public long? DeletedBy { get; set; }
    /// <summary>Gets or sets when the row was deleted.</summary>
    public DateTimeOffset? DeletedDate { get; set; }
    /// <summary>Gets or sets whether the row is soft deleted.</summary>
    public bool IsDeleted { get; set; }
    /// <summary>Gets or sets the optimistic concurrency version.</summary>
    public long RowVersion { get; set; }
}
