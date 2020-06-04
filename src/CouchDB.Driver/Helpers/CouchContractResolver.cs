using System;
using System.Reflection;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CouchDB.Driver.Helpers
{
    public class CouchContractResolver : DefaultContractResolver
    {
        private readonly PropertyCaseType _propertyCaseType;

        internal CouchContractResolver(PropertyCaseType propertyCaseType)
        {
            _propertyCaseType = propertyCaseType;
        }

        protected override JsonProperty? CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if (property != null && !property.Ignored)
            {
                property.PropertyName = member.GetCouchPropertyName(_propertyCaseType);
            }
            return property;
        }
    }
}
