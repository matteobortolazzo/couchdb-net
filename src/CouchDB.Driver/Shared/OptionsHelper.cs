using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace CouchDB.Driver.Shared
{
    internal static class OptionsHelper
    {
        public static IEnumerable<(string Name, object? Value)> ToQueryParameters(object options)
        {
            static TAttribute? GetAttribute<TAttribute>(ICustomAttributeProvider propertyInfo)
            {
                return propertyInfo
                    .GetCustomAttributes(typeof(TAttribute), true)
                    .Cast<TAttribute>()
                    .FirstOrDefault();
            }

            Type optionsType = options.GetType();
            foreach (PropertyInfo propertyInfo in optionsType.GetProperties())
            {
                JsonPropertyAttribute? jsonProperty = GetAttribute<JsonPropertyAttribute>(propertyInfo);
                DefaultValueAttribute? defaultValue = GetAttribute<DefaultValueAttribute>(propertyInfo);
                if (jsonProperty == null || defaultValue == null)
                {
                    continue;
                }

                var propertyName = jsonProperty.PropertyName
                                   ?? throw new ArgumentException($"JSON attribute not found for property {propertyInfo.Name}.");
                object propertyValue = propertyInfo.GetValue(options);
                object propertyDefaultValue = defaultValue.Value;

                var isDefault = Equals(propertyValue, propertyDefaultValue) ||
                                string.Equals(propertyValue?.ToString(), propertyDefaultValue?.ToString(), StringComparison.InvariantCultureIgnoreCase);
                if (isDefault)
                {
                    continue;
                }

                object? propertyStringValue = propertyValue?.ToString();
                if (propertyInfo.PropertyType == typeof(bool))
                {
                    propertyStringValue = propertyValue?.ToString().ToLowerInvariant();
                }

                yield return (propertyName, propertyStringValue);
            }
        }
    }
}