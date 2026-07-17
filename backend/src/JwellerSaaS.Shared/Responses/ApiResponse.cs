namespace JwellerSaaS.Shared.Responses;

/// <summary>Represents a consistent API response envelope.</summary>
public sealed record ApiResponse<T>(bool Success, T? Data, ErrorResponse? Error)
{
    /// <summary>Creates a successful response.</summary>
    public static ApiResponse<T> Ok(T data) => new(true, data, null);

    /// <summary>Creates a failed response.</summary>
    public static ApiResponse<T> Fail(ErrorResponse error) => new(false, default, error);
}
