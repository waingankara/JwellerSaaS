namespace Orion.Framework.Search;

/// <summary>Supported framework search operators.</summary>
public enum SearchOperator
{
    /// <summary>Contains text.</summary>
Contains,
    /// <summary>Starts with text.</summary>
StartsWith,
    /// <summary>Ends with text.</summary>
EndsWith,
    /// <summary>Equals value.</summary>
Equals,
    /// <summary>Does not equal value.</summary>
NotEquals,
    /// <summary>Greater than value.</summary>
GreaterThan,
    /// <summary>Greater than or equal value.</summary>
GreaterOrEqual,
    /// <summary>Less than value.</summary>
LessThan,
    /// <summary>Less than or equal value.</summary>
LessOrEqual,
    /// <summary>Between two values.</summary>
Between,
    /// <summary>In a set.</summary>
In,
    /// <summary>Not in a set.</summary>
NotIn,
    /// <summary>Is null.</summary>
IsNull,
    /// <summary>Is not null.</summary>
IsNotNull
}
