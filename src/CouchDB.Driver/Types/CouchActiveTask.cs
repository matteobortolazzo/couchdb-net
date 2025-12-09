#nullable disable
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
    public int ChangesDone { get; internal set; }

    /// <summary>
    /// Source database.
    /// </summary>
    [JsonPropertyName("database")]
    public string Database { get; internal set; }

    /// <summary>
    /// Process ID
    /// </summary>
    [JsonPropertyName("pid")]
    public string PID { get; internal set; }

    /// <summary>
    /// Current percentage progress.
    /// </summary>
    [JsonPropertyName("progress")]
    public int Progress { get; internal set; }

    /// <summary>
    /// Task start time.
    /// </summary>
    [JsonPropertyName("started_on")]
    [JsonConverter(typeof(MicrosecondEpochConverter))]
    public DateTime StartedOn { get; internal set; }

    /// <summary>
    /// Task status message.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; internal set; }

    /// <summary>
    /// Task name.
    /// </summary>
    [JsonPropertyName("task")]
    public string Task { get; internal set; }

    /// <summary>
    /// Total changes to process.
    /// </summary>
    [JsonPropertyName("total_changes-done")]
    public int TotalChanges { get; internal set; }

    /// <summary>
    /// Operation type.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; internal set; }

    /// <summary>
    /// Last operation update.
    /// </summary>
    [JsonPropertyName("updated_on")]
    [JsonConverter(typeof(MicrosecondEpochConverter))]
    public DateTime UpdatedOn { get; internal set; }
}
#nullable restore