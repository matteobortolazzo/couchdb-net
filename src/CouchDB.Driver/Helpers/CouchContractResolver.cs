using System.Reflection;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CouchDB.Driver.Helpers
{
    public class CouchContractResolver : DefaultContractResolver
    {
        private readonly PropertyCaseType _propertyCaseType;
        private readonly string? _databaseSplitDiscriminator;

        internal CouchContractResolver(PropertyCaseType propertyCaseType, string? databaseSplitDiscriminator)
        {
            _propertyCaseType = propertyCaseType;
            _databaseSplitDiscriminator = databaseSplitDiscriminator;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            Check.NotNull(member, nameof(member));

            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if (property is { Ignored: false })
            {
                if (member.DeclaringType!.Assembly != GetType().Assembly)
                {
                    property.PropertyName = member.GetCouchPropertyName(_propertyCaseType);
                }
            }

            if (property.PropertyName == CouchClient.DefaultDatabaseSplitDiscriminator && !string.IsNullOrWhiteSpace(_databaseSplitDiscriminator))
            {
                property.PropertyName = _databaseSplitDiscriminator;
            }
            return property;
        }
    }
}