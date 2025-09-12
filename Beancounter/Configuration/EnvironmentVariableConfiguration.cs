using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Beancounter.Extension;
using DotNetEnv;

namespace Beancounter.Configuration;

/// <summary>
/// Abstract base class for configuration objects that automatically load from environment variables.
/// Uses reflection to find properties marked with [EnvName] attributes and loads their values from environment variables.
/// </summary>
public abstract class EnvironmentVariableConfiguration
{
    /// <summary>
    /// Initializes a new instance of the EnvironmentVariableConfiguration class.
    /// Automatically loads all properties marked with [EnvName] attributes from environment variables.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when a required environment variable is not set.</exception>
    /// <exception cref="NotSupportedException">Thrown when an environment variable cannot be converted to the property type.</exception>
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
    
    /// <summary>
    /// Attempts to convert a string value to the specified type using TypeConverter.
    /// </summary>
    /// <param name="input">The string value to convert.</param>
    /// <param name="resultType">The target type to convert to.</param>
    /// <param name="result">The converted result if successful.</param>
    /// <returns>True if conversion was successful, false otherwise.</returns>
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

    /// <summary>
    /// Attribute used to mark properties that should be loaded from environment variables.
    /// </summary>
    public class EnvName : Attribute
    {
        /// <summary>
        /// Gets the name of the environment variable.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Gets the default value if the environment variable is not set.
        /// </summary>
        public string? DefaultValue { get; }
        
        /// <summary>
        /// Gets a value indicating whether a default value is specified.
        /// </summary>
        public bool HasDefaultValue { get; }
        
        /// <summary>
        /// Initializes a new instance of the EnvName attribute for an optional environment variable with a default value.
        /// </summary>
        /// <param name="name">The name of the environment variable.</param>
        /// <param name="defaultValue">The default value if the environment variable is not set.</param>
        public EnvName(string name, string? defaultValue)
        {
            Name = name;
            DefaultValue = defaultValue;
            HasDefaultValue = true;
        }

        /// <summary>
        /// Initializes a new instance of the EnvName attribute for a required environment variable.
        /// </summary>
        /// <param name="name">The name of the environment variable.</param>
        public EnvName(string name) {
            Name = name;
            DefaultValue = null;
            HasDefaultValue = false;
        }
    }
}

/// <summary>
/// Singleton class responsible for loading environment variables from both the system environment and .env files.
/// </summary>
file class VariableLoader
{
    private static readonly Lazy<VariableLoader> InstanceBackingField = new(() => new VariableLoader());

    /// <summary>
    /// Initializes a new instance of the VariableLoader class.
    /// Loads environment variables from system environment and .env files.
    /// </summary>
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

    /// <summary>
    /// Gets the singleton instance of the VariableLoader.
    /// </summary>
    public static VariableLoader Instance => InstanceBackingField.Value;

    private readonly Dictionary<string, string?> variables = new();

    /// <summary>
    /// Gets the value of an environment variable.
    /// </summary>
    /// <param name="key">The name of the environment variable.</param>
    /// <returns>The value of the environment variable, or null if not found.</returns>
    public string? GetEnvironmentVariable(string key) {
        _ = variables.TryGetValue(key, out var value);
        return value;
    }
}