using System.Text;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Indexes;

internal class IndexDefinition(Dictionary<string, IndexFieldDirection> fields, string? partialSelector)
{
    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append('{');

        // Partial Selector
        if (partialSelector != null)
        {
            sb.Append(partialSelector);
            sb.Append(',');
        }

        // Fields
        sb.Append("\"fields\":[");

        foreach ((var fieldName, IndexFieldDirection fieldDirection) in fields)
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