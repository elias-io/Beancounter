using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Beancounter.Extension;
using DotNetEnv;

namespace Beancounter.Configuration;

public abstract class EnvironmentVariableConfiguration
{
    protected EnvironmentVariableConfiguration()
    {
        var properties = GetType().GetProperties();
        foreach (var property in properties)
        {
            var optEnvAttr = property.GetCustomAttribute<EnvName>();
            if (optEnvAttr is not {} envAttr) continue;
            var envName = envAttr.Name;
            var envVar = VariableLoader.Instance.GetEnvironmentVariable(envName) ?? envAttr.DefaultValue;
            if (envVar is null && !envAttr.HasDefaultValue) {
                throw new ArgumentException($"Environment variable {envName} is not set!");
            } else if (envVar is null) {
                property.SetValue(this, null);
            } else if (TryConvert(envVar, property.PropertyType,  out var result)) {
                var cast = Convert.ChangeType(result, property.PropertyType);
                property.SetValue(this, cast);
            } else {
                throw new NotSupportedException(
                    $"Environment variable {envName} could not be converted to {property.PropertyType}!");
            }
        }
    }
    private static bool TryConvert(string input, Type resultType, out object result)
    {
        if (TypeDescriptor.GetConverter(resultType) is { } converter)
        {
            try
            {
                result = converter.ConvertFromString(input)!;
                return true;
            }
            catch (NotSupportedException)
            {
                result = default;
                return false;
            }
        }
        result = default;
        return false;

    }

    public class EnvName : Attribute
    {
        public string Name { get; }
        public string? DefaultValue { get; }
        public bool HasDefaultValue { get; }
        public EnvName(string name, string? defaultValue)
        {
            Name = name;
            DefaultValue = defaultValue;
            HasDefaultValue = true;
        }

        public EnvName(string name) {
            Name = name;
            DefaultValue = null;
            HasDefaultValue = false;
        }
    }
}

file class VariableLoader
{
    private static readonly Lazy<VariableLoader> InstanceBackingField = new(() => new VariableLoader());

    private VariableLoader() {
        foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables()) {
            if (environmentVariable is { Key: string key, Value: string value }
                && !string.IsNullOrWhiteSpace(value)) {
                variables.TryAdd(key, value);
            }
        }
        Env.Load(options: new LoadOptions(onlyExactPath: false))
            .ForEach(envVar => {
                variables.TryAdd(envVar.Key, envVar.Value);
            });
    }



    public static VariableLoader Instance => InstanceBackingField.Value;

    private readonly Dictionary<string, string?> variables = new();

    public string? GetEnvironmentVariable(string key) {
        _ = variables.TryGetValue(key, out var value);
        return value;
    }
}