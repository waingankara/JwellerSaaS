namespace Orion.Framework.Crud;

/// <summary>Identifies a data access operation handled by the Orion CRUD engine.</summary>
public enum CrudOperation
{
    /// <summary>Create an entity.</summary>
    Create,
    /// <summary>Update an entity.</summary>
    Update,
    /// <summary>Delete an entity permanently.</summary>
    Delete,
    /// <summary>Mark an entity as deleted.</summary>
    SoftDelete,
    /// <summary>Restore a soft-deleted entity.</summary>
    Restore,
    /// <summary>Read one entity by primary key.</summary>
    GetById,
    /// <summary>Read many entities.</summary>
    GetMany,
    /// <summary>Check for entity existence.</summary>
    Exists,
    /// <summary>Count entities.</summary>
    Count,
    /// <summary>Insert many entities.</summary>
    BulkInsert,
    /// <summary>Update many entities.</summary>
    BulkUpdate,
    /// <summary>Delete many entities permanently.</summary>
    BulkDelete,
    /// <summary>Mark many entities as deleted.</summary>
    BulkSoftDelete,
    /// <summary>Restore many soft-deleted entities.</summary>
    BulkRestore
}
