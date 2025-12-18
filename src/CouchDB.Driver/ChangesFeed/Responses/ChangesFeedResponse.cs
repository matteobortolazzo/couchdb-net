namespace CouchDB.Driver.ChangesFeed.Responses;

[Serializable]
public class ChangesFeedResponse<TSource> where TSource : class
{
    [property:JsonPropertyName("last_seq")]
    public required string LastSequence { get; init; }

    [property:JsonPropertyName("pending")]
    public int Pending { get; init; }

    [property:JsonPropertyName("results")]
    public required IList<ChangesFeedResponseResult<TSource>> Results { get; set; }
}