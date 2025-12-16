using CouchDB.Driver.Helpers;

namespace CouchDB.Driver.Types;

/// <summary>
/// Represents an active task
/// </summary>
/// <param name="ChangesDone">Processes changes.</param>
/// <param name="Database">Source database.</param>
/// <param name="PID">Process ID</param>
/// <param name="Progress">Current percentage progress.</param>
/// <param name="StartedOn">Task start time.</param>
/// <param name="Status">Task status message.</param>
/// <param name="Task">Task name.</param>
/// <param name="TotalChanges">Total changes to process.</param>
/// <param name="Type">Operation type.</param>
/// <param name="UpdatedOn">Last operation update.</param>
[Serializable]
public sealed record CouchActiveTask(
    [property: JsonPropertyName("changes-done")]
    int ChangesDone,
    [property: JsonPropertyName("database")]
    string Database,
    [property: JsonPropertyName("pid")]
    string PID,
    [property: JsonPropertyName("progress")]
    int Progress,
    [property: JsonPropertyName("started_on")]
    [property: JsonConverter(typeof(MicrosecondEpochConverter))]
    DateTimeOffset StartedOn,
    [property: JsonPropertyName("status")]
    string? Status,
    [property: JsonPropertyName("task")]
    string? Task,
    [property: JsonPropertyName("total_changes-done")]
    int TotalChanges,
    [property: JsonPropertyName("type")]
    string Type,
    [property: JsonPropertyName("updated_on")]
    [property: JsonConverter(typeof(MicrosecondEpochConverter))]
    DateTimeOffset UpdatedOn
);