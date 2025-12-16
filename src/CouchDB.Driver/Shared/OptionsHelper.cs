using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Reflection;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Shared;

internal static class OptionsHelper
{
    public static IEnumerable<(string Name, object? Value)> ToQueryParameters(object options)
    {
        Type optionsType = options.GetType();
        foreach (PropertyInfo propertyInfo in optionsType.GetProperties())
        {
            JsonPropertyNameAttribute? jsonProperty = GetAttribute<JsonPropertyNameAttribute>(propertyInfo);
            DefaultValueAttribute? defaultValue = GetAttribute<DefaultValueAttribute>(propertyInfo);
            if (jsonProperty == null || defaultValue == null)
            {
                continue;
            }

            var propertyName = jsonProperty.Name
                               ?? throw new ArgumentException(
                                   $"JSON attribute not found for property {propertyInfo.Name}.");
            var propertyValue = propertyInfo.GetValue(options);
            var propertyDefaultValue = defaultValue.Value;

            var isDefault = Equals(propertyValue, propertyDefaultValue) ||
                            string.Equals(propertyValue?.ToString(), propertyDefaultValue?.ToString(),
                                StringComparison.InvariantCultureIgnoreCase);
            if (isDefault)
            {
                continue;
            }

            object? propertyStringValue = propertyValue?.ToString();
            if (propertyInfo.PropertyType == typeof(bool))
            {
                propertyStringValue = propertyValue?.ToString()?.ToLowerInvariant();
            }

            yield return (propertyName, propertyStringValue);
        }

        yield break;

        static TAttribute? GetAttribute<TAttribute>(ICustomAttributeProvider propertyInfo)
        {
            return propertyInfo
                .GetCustomAttributes(typeof(TAttribute), true)
                .Cast<TAttribute>()
                .FirstOrDefault();
        }
    }
}