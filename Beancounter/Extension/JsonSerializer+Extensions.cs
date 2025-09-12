using Newtonsoft.Json;

namespace Beancounter.Extension;

/// <summary>
/// Provides extension methods for JsonSerializer operations.
/// </summary>
public static class JsonSerializerExtensions
{
    /// <summary>
    /// Serializes an object to a JSON string using the JsonSerializer instance.
    /// </summary>
    /// <param name="jsonSerializer">The JsonSerializer instance to use.</param>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>JSON string representation of the object.</returns>
    public static string Serialize(this JsonSerializer jsonSerializer, object obj)
    {
        using var stringWriter = new StringWriter();
        jsonSerializer.Serialize(stringWriter, obj);
        var jsonString = stringWriter.ToString();
        return jsonString;
    }
}