using System.Text.Json;
using CouchDB.Driver.DTOs;
using CouchDB.Driver.Local;
using CouchDB.Driver.Security;
using CouchDB.Driver.Types;

namespace CouchDB.Driver;

[JsonSourceGenerationOptions]
// Internal
[JsonSerializable(typeof(AttachmentResult))]
[JsonSerializable(typeof(CouchError))]
[JsonSerializable(typeof(CreateAttachmentRequest))]
[JsonSerializable(typeof(Dictionary<string, CreateAttachmentRequest>))]
[JsonSerializable(typeof(CreateIndexResult))]
[JsonSerializable(typeof(GetIndexesResult))]
[JsonSerializable(typeof(IndexDefinitionInfo))]
[JsonSerializable(typeof(OperationResult))]
[JsonSerializable(typeof(ReplicationHost))]
[JsonSerializable(typeof(StatusResult))]
[JsonSerializable(typeof(LocalDocumentsResult))]
// Public
[JsonSerializable(typeof(Cluster))]
[JsonSerializable(typeof(ActiveTask))]
[JsonSerializable(typeof(ActiveTask[]))]
[JsonSerializable(typeof(CreateDatabaseOptions))]
[JsonSerializable(typeof(DatabaseInfo))]
[JsonSerializable(typeof(DatabaseProps))]
[JsonSerializable(typeof(PartitionInfo))]
[JsonSerializable(typeof(ReplicationRequest))]
[JsonSerializable(typeof(ReplicationAuth))]
[JsonSerializable(typeof(ReplicationAuth))]
[JsonSerializable(typeof(ReplicationBasicCredentials))]
[JsonSerializable(typeof(DatabaseUser))]
[JsonSerializable(typeof(ExecutionStats))]
[JsonSerializable(typeof(IndexInfo))]
[JsonSerializable(typeof(ReadItemAttachment))]
[JsonSerializable(typeof(RevisionInfo))]
[JsonSerializable(typeof(Sizes))]
[JsonSerializable(typeof(WriteItemResponse))]
[JsonSerializable(typeof(BulkWriteItemResponse))]
[JsonSerializable(typeof(BulkWriteItemResponse[]))]
// Security
[JsonSerializable(typeof(DatabaseSecurityInfo))]
internal partial class CouchSourceGenerationContext : JsonSerializerContext
{
}

public static class CouchJsonSerializerOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        TypeInfoResolver = CouchSourceGenerationContext.Default
    };
}