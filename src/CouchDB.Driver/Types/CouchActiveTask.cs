using System;
using CouchDB.Driver.Helpers;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Types;

/// <summary>
/// Represents an active task.
/// </summary>
[Serializable]
public sealed class CouchActiveTask
{
    /// <summary>
    /// Processes changes.
    /// </summary>
    [JsonPropertyName("changes-done")]
    public int ChangesDone { get; internal init; }

    /// <summary>
    /// Source database.
    /// </summary>
    [JsonPropertyName("database")]
    public required string Database { get; init; }

    /// <summary>
    /// Process ID
    /// </summary>
    [JsonPropertyName("pid")]
    public required string PID { get; init; }

    /// <summary>
    /// Current percentage progress.
    /// </summary>
    [JsonPropertyName("progress")]
    public int Progress { get; internal init; }

    /// <summary>
    /// Task start time.
    /// </summary>
    [JsonPropertyName("started_on")]
    [JsonConverter(typeof(MicrosecondEpochConverter))]
    public DateTimeOffset StartedOn { get; internal init; }

    /// <summary>
    /// Task status message.
    /// </summary>
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    /// <summary>
    /// Task name.
    /// </summary>
    [JsonPropertyName("task")]
    public required string Task { get; init; }

    /// <summary>
    /// Total changes to process.
    /// </summary>
    [JsonPropertyName("total_changes-done")]
    public int TotalChanges { get; internal init; }

    /// <summary>
    /// Operation type.
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// Last operation update.
    /// </summary>
    [JsonPropertyName("updated_on")]
    [JsonConverter(typeof(MicrosecondEpochConverter))]
    public DateTimeOffset UpdatedOn { get; internal init; }
}