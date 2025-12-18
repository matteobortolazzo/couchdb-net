using System.Text;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Indexes;

internal class IndexDefinition(Dictionary<string, IndexFieldDirection> fields, string? partialSelector)
{
    public Dictionary<string, IndexFieldDirection> Fields { get; } = fields;
    public string? PartialSelector { get; } = partialSelector;

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append('{');

        // Partial Selector
        if (PartialSelector != null)
        {
            sb.Append(PartialSelector);
            sb.Append(',');
        }

        // Fields
        sb.Append("\"fields\":[");

        foreach ((var fieldName, IndexFieldDirection fieldDirection) in Fields)
        {
            var fieldString = fieldDirection == IndexFieldDirection.Ascending
                ? $"\"{fieldName}\","
                : $"{{\"{fieldName}\":\"desc\"}},";

            sb.Append(fieldString);
        }

        sb.Length--;
        sb.Append("]}");

        return sb.ToString();
    }
}