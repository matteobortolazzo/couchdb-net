
using CouchDB.Driver.DTOs;

namespace CouchDB.Driver.Types;
// TODO: Review
/// <summary>
/// Represent info about the index.
/// </summary>
[Serializable]
public class IndexInfo
{
    /// <summary>
    /// ID of the design document the index belongs to.
    /// </summary>
    [property:JsonPropertyName("ddoc")]
    public required string DesignDocument { get; init; }

    /// <summary>
    /// The name of the index.
    /// </summary>
    [property:JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// The fields used in the index
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, IndexFieldDirection> Fields { get; } = new();

    [property:JsonPropertyName("def")]
    internal IndexDefinitionInfo Definition
    {
        set
        {
            Fields.Clear();

            foreach (Dictionary<string, string> definitions in value.Fields)
            {
                var (name, direction) = definitions.First();
                IndexFieldDirection fieldDirection = direction == "asc"
                    ? IndexFieldDirection.Ascending
                    : IndexFieldDirection.Descending;
                Fields.Add(name, fieldDirection);
            }
        }
    }
}