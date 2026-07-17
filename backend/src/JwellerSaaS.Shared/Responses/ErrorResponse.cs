namespace JwellerSaaS.Shared.Responses;

/// <summary>Describes an API error.</summary>
public sealed record ErrorResponse(string Code, string Message, IReadOnlyDictionary<string, string[]>? Details = null);
