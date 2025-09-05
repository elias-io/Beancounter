namespace Beancounter.Extension;

/// <summary>
/// Provides extension methods for Guid operations.
/// </summary>
public static class GuidExtension
{
    /// <summary>
    /// Converts a GUID to its Base64 string representation.
    /// </summary>
    /// <param name="guid">The GUID to convert.</param>
    /// <returns>Base64 encoded string representation of the GUID.</returns>
    public static string ToBase64String(this Guid guid)
    {
        return Convert.ToBase64String(guid.ToByteArray());
    }
    
    /// <summary>
    /// Converts a GUID to a plain string without hyphens.
    /// </summary>
    /// <param name="guid">The GUID to convert.</param>
    /// <returns>GUID string without hyphens.</returns>
    public static string ToPlainString(this Guid guid)
    {
        return guid.ToString("N");
    }
}