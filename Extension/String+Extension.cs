using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Beancounter.Extension;

/// <summary>
/// Provides extension methods for string operations.
/// </summary>
public static class String_Extension {

    /// <summary>
    /// Converts a string to ASCII by removing diacritics and special characters.
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <returns>The ASCII version of the input string.</returns>
    public static string ConvertToAscii(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Normalize to FormD to separate base characters and diacritics
        var normalized = input.Normalize(NormalizationForm.FormD);

        // Remove diacritic characters
        var sb = new StringBuilder();
        foreach (char c in normalized)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        // Normalize back to FormC
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Converts a string to a URL-safe identifier by removing special characters and normalizing.
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <returns>A URL-safe identifier.</returns>
    public static string ToStringId(this string input) {
        var result = input;
        result = result.Replace(" ", "_");
        result = result.ToLower();
        result = result.ConvertToAscii();
        // Replace non a-zA-Z0-9 with _
        result = Regex.Replace(result, @"[^a-zA-Z0-9]", "_");
        result = Regex.Replace(result, @"_+", "_");
        result = result.TrimEnd('_');
        return result;
    }
}