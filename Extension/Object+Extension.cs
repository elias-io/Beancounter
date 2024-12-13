using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Beancounter.Extension;
public static class ObjectExtension
{
    public static bool Validate(this object? obj, out List<string> validationErrors)
    {
        validationErrors = new List<string>();
        return ValidateObject(obj, validationErrors);
    }

    private static bool ValidateObject(object? obj, List<string> validationErrors, string path = "")
    {
        if (obj == null) return true;
        var properties = obj.GetType().GetProperties();
        var isValid = true;
        foreach (var property in properties)
        {
            var value = property.GetValue(obj);
            var propertyPath = string.IsNullOrEmpty(path) ? property.Name : $"{path}.{property.Name}";
            if (!IsNullable(property) && value == null)
            {
                validationErrors.Add($"Property '{propertyPath}' is non-nullable but is null.");
                isValid = false;
            }
            if (value != null && !property.PropertyType.IsPrimitive && !(value is string))
            {
                isValid &= ValidateObject(value, validationErrors, propertyPath);
            }
        }

        return isValid;
    }

    private static bool IsNullable(PropertyInfo property)
    {
        var type = property.PropertyType;
        if (Nullable.GetUnderlyingType(type) != null) return true;
        if (!type.IsValueType)
        {
            var nullableAttribute = property.CustomAttributes.FirstOrDefault(attr =>
                attr.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");

            if (nullableAttribute != null)
            {
                var nullableFlag = (byte?)nullableAttribute.ConstructorArguments.FirstOrDefault().Value;
                return nullableFlag == 2;
            }
            return false;
        }
        return false;
    }

}
