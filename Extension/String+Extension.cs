using System.Globalization;
using System.Text;

namespace Beancounter.Extension;

public static class String_Extension {

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
}