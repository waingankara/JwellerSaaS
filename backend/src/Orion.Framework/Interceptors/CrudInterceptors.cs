using Orion.Framework.Crud;

namespace Orion.Framework.Interceptors;

/// <summary>Intercepts CRUD execution.</summary>
public interface ICrudInterceptor
{
    /// <summary>Called before SQL execution.</summary>
    Task BeforeExecuteAsync(CrudContext context, CancellationToken cancellationToken) => Task.CompletedTask;
    /// <summary>Called after SQL execution.</summary>
    Task AfterExecuteAsync(CrudContext context, CancellationToken cancellationToken) => Task.CompletedTask;
    /// <summary>Called after successful operation.</summary>
    Task OnSuccessAsync(CrudContext context, CancellationToken cancellationToken) => Task.CompletedTask;
    /// <summary>Called after failed operation.</summary>
    Task OnFailureAsync(CrudContext context, Exception exception, CancellationToken cancellationToken) => Task.CompletedTask;
}
