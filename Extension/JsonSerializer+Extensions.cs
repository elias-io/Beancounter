using Newtonsoft.Json;

namespace Beancounter.Extension;

public static class JsonSerializerExtensions
{
    public static string Serialize(this JsonSerializer jsonSerializer, object obj)
    {
        using var stringWriter = new StringWriter();
        jsonSerializer.Serialize(stringWriter, obj);
        var jsonString = stringWriter.ToString();
        return jsonString;
    }
}