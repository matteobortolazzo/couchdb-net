namespace CouchDB.Driver.Types;

// TODO: Review
[Serializable]
public class Revisions
{
    [property:JsonPropertyName("start")]
    public int Start { get; init; }

    [property:JsonIgnore]
    public IReadOnlyCollection<string> IDs { get; private set; } = null!;

    [property:JsonPropertyName("ids")]
    private List<string> IdsOther { set { IDs = value.AsReadOnly(); } }
}