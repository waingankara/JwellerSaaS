namespace Orion.Framework.Branching;

public sealed record BranchContext(long? BranchId);
public interface IBranchContextAccessor { BranchContext Current { get; } }
