using System.Text.Json.Serialization;
using CouchDB.Driver.E2ETests.Models;

namespace CouchDB.Driver.E2ETests;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(Rebel))]
[JsonSerializable(typeof(RebelSettings))]
internal partial class SourceGenerationContext : JsonSerializerContext { }
