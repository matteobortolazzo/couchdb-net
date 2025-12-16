namespace CouchDB.Driver.ChangesFeed.Responses;

[Serializable]
public class ChangesFeedResponseResultChange
{
    [property:JsonPropertyName("rev")]
    public required string Rev { get; init; }
}