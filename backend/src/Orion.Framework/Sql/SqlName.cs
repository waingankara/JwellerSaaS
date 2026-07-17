using System.Text.RegularExpressions;

namespace Orion.Framework.Sql;

/// <summary>Validates SQL identifiers used by framework builders.</summary>
public static partial class SqlName
{
    /// <summary>Validates and returns a SQL identifier.</summary>
    public static string Identifier(string value)
    {
        if (!IdentifierRegex().IsMatch(value)) throw new ArgumentException("Invalid SQL identifier.", nameof(value));
        return value;
    }

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$")]
    private static partial Regex IdentifierRegex();
}
