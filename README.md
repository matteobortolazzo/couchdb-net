[![Downloads](https://img.shields.io/nuget/dt/CouchDB.NET.svg)](https://www.nuget.org/packages/CouchDB.NET/)

# CouchDB.NET V4

Query **CouchDB** with *LINQ*! Inspired by *Cosmos SDK*.

For CouchDB.NET V3 docs: [README.md](README-V3.md)

## LINQ queries

C# query example:

```csharp
// Setup
var clientOptions = new CouchClientOptions
{
    JsonSerializerOptions = JsonSerializerOptions.Web
};
var client = new CouchClient(
    endpoint: "http://localhost:5984",
    credentials: new BasicCredentials("admin", "admin"),
    clientOptions);
var rebels = await client.GetDatabase<Rebel>("rebels");

// Usage
var skywalkers = await rebels
    .Where(r => 
        r.Surname == "Skywalker" && 
        (
            r.Battles.All(b => b.Planet == "Naboo") ||
            r.Battles.Any(b => b.Planet == "Death Star")
        )
    )
    .OrderByDescending(r => r.Name)
    .ThenByDescending(r => r.Age)
    .Take(2)
    .Select(
        r => r.Name,
        r => r.Age
    })
    .ToListAsync();
```

The produced Mango JSON:

```json
{
  "selector": {
    "$and": [
      {
        "surname": "Skywalker"
      },
      {
        "$or": [
          {
            "battles": {
              "$allMatch": {
                "planet": "Naboo"
              }
            }
          },
          {
            "battles": {
              "$elemMatch": {
                "planet": "Death Star"
              }
            }
          }
        ]
      }
    ]
  },
  "sort": [
    {
      "name": "desc"
    },
    {
      "age": "desc"
    }
  ],
  "limit": 2,
  "fields": [
    "name",
    "age"
  ]
}
```

## Index

* [Getting started](#getting-started)
* [Queries](#queries)
    * [Combinations](#combinations)
    * [Conditions](#conditions)
    * [Native method](#native-methods)
    * [Composite methods](#composite-methods)
* [Authentication](#authentication)
* [Options](#options)
* [Serialization behavior](#serialization-behavior)
* [HttpClient customization](#httpclient-customization)
* [Changes Feed](#changes-feed)
    * [Feed Options](#feed-options)
    * [Feed Filter](#feed-filter)
* [Indexing](#indexing)
    * [Index Options](#index-options)
    * [Partial Indexes](#partial-indexes)
* [Partitioned Databases](#partitioned-databases)
* [Views](#views)
* [Local (non-replicating) Documents](#local-non-replicating-documents)
* [Bookmark and Execution stats](#bookmark-and-execution-stats)
* [Users](#users)
* [Replication](#replication)
* [Dependency Injection](#dependency-injection)

## Getting started

* Install it from NuGet: [https://www.nuget.org/packages/CouchDB.NET](https://www.nuget.org/packages/CouchDB.NET)
* Create a client providing the endpoint and authentication:
  ```csharp
  var client = new CouchClient(
    endpoint: "http://localhost:5984",
    credentials: new BasicCredentials("admin", "admin"))
  ```
* Create a class or record for your documents:
  ```csharp
  public record Rebel(string Id, string Name)
  {
     public string Rev { get; init; } = null!;
  }
  ```
* Get a database reference:
  ```csharp
  var rebels = client.GetDatabase<Rebel>("rebels");
  ```
* Query the database
  ```csharp
  var skywalkers = await rebels.Where(r => r.Surname == "Skywalker").ToListAsync();
  ```

## Queries

The database class exposes all the implemented *LINQ* methods like Where and OrderBy,
those methods returns an `IQueryable`.

*LINQ* are supported natively to the following is possible:

```csharp
var skywalkers =
    from r in fixture.Rebels
    where r.Surname == "Skywalker"
    select r;
```

### Selector

The selector is created when the method Where (`IQueryable`) is called.
If the `Where` method is not called in the expression, it will at an empty selector.

#### Combinations

| Mango      | C#               |
|:-----------|:-----------------|
| $and       | &&               |
| $or        | \|\|             |
| $not       | !                |
| $nor       | !( \|\| )        |
| $all       | a.Contains(x)    |
| $all       | a.Contains(list) |
| $elemMatch | a.Any(condition) |
| $allMatch  | a.All(condition) |

#### Conditions

| Mango          | C#                 |
|:---------------|:-------------------|
| $lt            | <                  |
| $lte           | <=                 |
| $eq (implicit) | ==                 |
| $ne            | !=                 |
| $gte           | >=                 |
| $gt            | >                  |
| $exists        | o.FieldExists(s)   |
| $type          | o.IsCouchType(...) |
| $in            | o.In(list)         |
| $nin           | !o.In(list)        |
| $size          | a.Count == x       |
| $mod           | n % x = y          |
| $regex         | s.IsMatch(rx)      |

### Native methods

| Mango           | C#                                                   |
|:----------------|:-----------------------------------------------------|
| limit           | Take(n)                                              |
| skip            | Skip(n)                                              |
| sort            | OrderBy(..)                                          |
| sort            | OrderBy(..).ThenBy()                                 |
| sort            | OrderByDescending(..)                                |
| sort            | OrderByDescending(..).ThenByDescending()             |
| fields          | Select(x => x.Prop1, x => x.Prop2)                   |
| fields          | Convert<SourceType, SimplerType>()                   |
| use_index       | UseIndex("design_document")                          |
| use_index       | UseIndex(new [] { "design_document", "index_name" }) |
| r               | WithReadQuorum(n)                                    |
| bookmark        | UseBookmark(s)                                       |
| update          | WithoutIndexUpdate()                                 |
| stable          | FromStable()                                         |
| execution_stats | IncludeExecutionStats()                              |
| conflicts       | IncludeConflicts()                                   |

### Composite methods

Some methods that are not directly supported by *CouchDB* are converted to a composition of supported ones!

| Input                           | Output                                                                   |
|:--------------------------------|:-------------------------------------------------------------------------|
| Min(d => d.Property)            | OrderBy(d => d.Property).Take(1).Select(d => d.Property).Min()           |
| Max(d => d.Property)            | OrderByDescending(d => d.Property).Take(1).Select(d => d.Property).Max() |
| Sum(d => d.Property)            | Select(d => d.Property).Sum()                                            |
| Average(d => d.Property)        | Select(d => d.Property).Average()                                        |
| Any()                           | Take(1).Any()                                                            |
| Any(d => condition)             | Where(d => condition).Take(1).Any()                                      |
| All(d => condition)             | Where(d => !condition).Take(1).Any()                                     |
| Single()                        | Take(2).Single()                                                         |
| SingleOrDefault()               | Take(2).SingleOrDefault()                                                |
| Single(d => condition)          | Where(d => condition).Take(2).Single()                                   |
| SingleOrDefault(d => condition) | Where(d => condition).Take(2).SingleOrDefault()                          |
| First()                         | Take(1).First()                                                          |
| FirstOrDefault()                | Take(1).FirstOrDefault()                                                 |
| First(d => condition)           | Where(d => condition).Take(1).First()                                    |
| FirstOrDefault(d => condition)  | Where(d => condition).Take(1).FirstOrDefault()                           |
| Last()                          | Where(d => Last()                                                        |
| LastOrDefault()                 | LastOrDefault()                                                          |
| Last(d => condition)            | Where(d => condition).Last()                                             |
| LastOrDefault(d => condition)   | Where(d => condition).LastOrDefault()                                    |

**INFO**: Also `Select(d => d.Property)`, `Min` and `Max` are supported.

**WARN**: Since Max and Min use **sort**, an *index* must be created.

### All other `IQueryable` methods

`IQueryable` methods that are not natively supported will throw an **exception**.

## Authentication

There are four types of authentication supported: Basic, Cookie, Proxy and JWT to pass in the `CouchClient` constructor.

```csharp
// Basic
var cred = new BasicCredentials("root", "relax");

// Cookie
var cred = new CookieCredentials("root", "relax");
var cred = new CookieCredentials("root", "relax", cookieDuration);

// Proxy
var cred = new ProxyCredentials("root", Roles: ["role1", "role2"])

// JTW
var cred = new JwtCredentials("token")
var cred = new JwtCredentials(async () => await NewTokenAsync())
```

## Options

Options can be specified when creating the `CouchClient`:

```csharp
var clientOptions = new CouchClientOptions
{
    // By default, if a warning is returned from CouchDB, an exception is thrown.
    // It can be enabled/disabled per query by calling .With/WithoutQueryWarningException() in LINQ
    ThrowOnQueryWarning = true,
    // By default, the SDK uses `JsonSerializerOptions.Web` for documents
    JsonSerializerOptions = myCustomJsonSerializerOptions,
    // Custom HttpClient used by the SDK
    HttpClient = myCustomHttpClient
}
```

## Serialization behavior

While the SDK aims to be as close to the APIs as possible, some work has been done to improve the developer experience.

* `Id` and `Rev` properties are mapped to `_id` and `_rev` fields automatically.
* `ReadItemAsync` return a `ReadItemResponse<T>` to separate the document from metadata.
* When updating a document, the `Rev` property is stripped automatically, and the method parameter is used instead.
* The `JsonSerializerOptions` provided are only used for user documents. For *SDK* objects, different options are used.
  They can't be changed, and they use source generators for better performance.

## HttpClient customization

You have full control over the `HttpClient` used by the SDK. The SDK generates the correct request, serialize and deserialize. 
Any other behavior can be customized by providing your own `HttpClient` instance in the `CouchClientOptions`.

## Changes Feed

The following *feed modes* are supported: `normal`, `longpool` and `continuous`.
Also, all *options* and *filter types* are supported.

`Continuous mode` is probably the most useful, and it's implemented with the new `IAsyncEnumerable`.

```csharp
var tokenSource = new CancellationTokenSource();
await foreach (var change in _rebels.GetContinuousChangesAsync(options: null, filter: null, tokenSource.Token))
{
    if (/* ... */) {
      tokenSource.Cancel();
    }
}
```

### Feed Options

```csharp
// Example
var options = new ChangesFeedOptions
{
  Descending = true,
  Limit = 10,
  Since = "now",
  IncludeDocs = true
};
ChangesFeedResponse<Rebel> changes = await GetChangesAsync(options);
```

### Feed Filter

```csharp
// _doc_ids
var filter = ChangesFeedFilter.DocumentIds(new[] { "luke", "leia" });
// _selector
var filter = ChangesFeedFilter.Selector<Rebel>(rebel => rebel.Age == 19);
// _design
var filter = ChangesFeedFilter.Design();
// _view
var filter = ChangesFeedFilter.View(view);
// Design document filter with custom query parameters
var filter = ChangesFeedFilter.DesignDocument("replication/by_partition", 
    new Dictionary<string, string> { { "partition", "skywalker" } });

// Use
ChangesFeedResponse<Rebel> changes = await GetChangesAsync(options: null, filter);
```

#### Design Document Filters with Query Parameters

For partitioned databases or custom filtering logic, you can use design document filters with query parameters:

```csharp
// Create a design document in CouchDB with a filter function
// _design/replication
{
  "filters": {
    "by_partition": function(doc, req) {
      var partition = req.query.partition;
      return doc._id.indexOf(partition + ':') === 0;
    }
  }
}

// Use the filter with query parameters
var filter = ChangesFeedFilter.DesignDocument("replication/by_partition", 
    new Dictionary<string, string> { { "partition", "businessId123" } });

await foreach (var change in db.GetContinuousChangesAsync(null, filter, cancellationToken))
{
    // Process changes from specific partition
}

// Or pass query parameters via options
var options = new ChangesFeedOptions
{
    Filter = "replication/by_partition",
    QueryParameters = new Dictionary<string, string> { { "partition", "businessId123" } }
};
var changes = await db.GetChangesAsync(options);
```

## Indexing

It is possible to create indexes to use when querying.

```csharp
// Basic index creation
await _rebels.CreateIndexAsync("rebels_index", b => b
    .IndexBy(r => r.Surname))
    .ThenBy(r => r.Name));

// Descending index creation
await _rebels.CreateIndexAsync("rebels_index", b => b
    .IndexByDescending(r => r.Surname))
    .ThenByDescending(r => r.Name));
```

### Index Options

```csharp
// Specifies the design document and/or whether a JSON index is partitioned or global
await _rebels.CreateIndexAsync("rebels_index", b => b
    .IndexBy(r => r.Surname),
    new IndexOptions()
    {
        DesignDocument = "surnames_ddoc",
        Partitioned = true
    });
```

### Partial Indexes

```csharp
// Create an index which excludes documents at index time
await _rebels.CreateIndexAsync("skywalkers_index", b => b
    .IndexBy(r => r.Name)
    .Where(r => r.Surname == "Skywalker");
```

### Indexes operations

```csharp
// Get the list of indexes
var indexes = await _rebels.GetIndexesAsync();

// Delete an indexes
await _rebels.DeleteIndexAsync(indexes[0]);
// or
await _rebels.DeleteIndexAsync("surnames_ddoc", name: "surnames");
```

## Partitioned Databases

CouchDB partitioned databases allow you to optimize query performance by grouping related documents together using a
partition key. 

### Creating a Partitioned Database

```csharp
// Create a partitioned database
var rebels = await client.CreateDatabaseAsync<Rebel>("rebels", partitioned: true);

// Or with GetOrCreateDatabaseAsync
var rebels = await client.GetOrCreateDatabaseAsync<Rebel>("rebels", partitioned: true);
```

### Partitioned Document IDs

In partitioned databases, document IDs must follow the format: `{partition_key}:{document_id}`

```csharp
var luke = new Rebel 
{ 
    Id = "skywalker:luke",  // partition key is "skywalker"
    Name = "Luke", 
    Surname = "Skywalker" 
};
await rebels.AddAsync(luke);
```

### Getting Partition Information

```csharp
// Get metadata about a specific partition
var partitionInfo = await rebels.GetPartitionInfoAsync("skywalker");
Console.WriteLine($"Documents in partition: {partitionInfo.DocCount}");
Console.WriteLine($"Partition size: {partitionInfo.Sizes.Active} bytes");
```

### Querying Partitioned Databases

Partition-specific queries are more efficient as they only scan documents within the partition:

```csharp
// Query a partition using Mango selector
var skywalkers = await rebels.QueryPartitionAsync("skywalker", new
{
    selector = new { name = new { $gt = "A" } },
    sort = new[] { "name" }
});

// Or with JSON string
var json = "{\"selector\": {\"name\": {\"$gt\": \"A\"}}}";
var results = await rebels.QueryPartitionAsync("skywalker", json);

// Get all documents in a partition
var allSkywalkers = await rebels.GetPartitionAllDocsAsync("skywalker");
```

### Checking if Database is Partitioned

```csharp
var dbInfo = await rebels.GetInfoAsync();
bool isPartitioned = dbInfo.Props?.Partitioned ?? false;
```

### Partitioned Indexes

When creating indexes for partitioned databases, specify whether the index should be partitioned or global:

```csharp
// Create a partitioned index (default for partitioned databases)
await rebels.CreateIndexAsync("name_index", 
    b => b.IndexBy(r => r.Name),
    new IndexOptions { Partitioned = true });

// Create a global index (queries across all partitions)
await rebels.CreateIndexAsync("global_index", 
    b => b.IndexBy(r => r.Age),
    new IndexOptions { Partitioned = false });
```

## Views

It's possible to query a view with the following:

```csharp
var options = new CouchViewOptions<string[]>
{
    StartKey = new[] {"Luke", "Skywalker"},
    IncludeDocs = true
};
var viewRows = await _rebels.GetViewAsync<string[], RebelView>("jedi", "by_name", options);
```

You can also query a view with multiple options to get multiple results:

```csharp
var lukeOptions = new CouchViewOptions<string[]>
{
    Key = new[] {"Luke", "Skywalker"},
    IncludeDocs = true
};
var yodaOptions = new CouchViewOptions<string[]>
{
    Key = new[] {"Yoda"},
    IncludeDocs = true
};
var queries = new[]
{
    lukeOptions,
    yodaOptions
};

var results = await _rebels.GetViewQueryAsync<string[], RebelView>("jedi", "by_name", queries);
var lukeRows = results[0];
var yodaRows = results[1];
```

## Local (non-replicating) Documents

The Local (non-replicating) document interface allows you to create local documents that are not replicated to other
databases.

```csharp
var docId = "settings";
var settings = new RebelSettings
{
    Id = docId,
    IsActive = true
};

// Create
await _rebels.LocalDocuments.CreateOrUpdateAsync(settings);

// Get by ID
settings = await _rebels.LocalDocuments.GetJsonAsync<RebelSettings>(docId);

// Get all
var docs = await local.GetAsync();

// Search
var searchOpt = new LocalDocumentsOptions
{
    Descending = true,
    Limit = 10,
    Conflicts = true
};
var docs = await local.GetAsync(searchOpt);
```

### Bookmark and Execution stats

Bookmark and execution stats can be found in the `CouchList<T>` result from `ToListAsync`.

```csharp
var allRebels = await rebels.ToListAsync();

foreach(var r in allRebels) 
{
    ...
}
var b = allRebels.Bookmark;
var ex = allRebels.ExecutionStats; // .IncludeExecutionStats() must be called
```

### Users

The driver natively support the *_users* database.

```csharp
var users = client.GetUsersDatabase();
var luke = await users.CreateAsync(new CouchUser(name: "luke", password: "lasersword"));
```

It's possible to extend *CouchUser* for store custom info.

```csharp
var users = client.GetUsersDatabase<CustomUser>();
var luke = await users.CreateAsync(new CustomUser(name: "luke", password: "lasersword"));
```

To change password:

```csharp
luke = await users.ChangeUserPassword(luke, "r2d2");
```

### Replication

The driver provides the ability to configure and cancel replication between databases.

```csharp
var options = new ConfigureReplicationOptions 
{
    Continuous = true
}
var success = await client.ConfigureReplicationAsync("anakin", "jedi", options);
if (success)
{ 
    await client.CancelReplicationAsync("anakin", "jedi");
}
```

It is also possible to specify a selector to apply to the replication

```csharp
new ConfigureReplicationOptions { Selector = new { designation = "FN-2187" } };
```

Credentials can be specified as follows

```csharp
new ConfigureReplicationOptions 
{
    SourceCredentials = new CouchReplicationBasicCredentials(username: "luke", password: "r2d2")
};
```

## Dependency Injection

`CouchClient`, and `CouchDatabase<T>` must be registered as a singleton services.

There is no built-in extension method, but you can create your own:

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCouchClient(
      this IServiceCollection services,
      string endpoint,
      CouchCredentials credentials,
      CouchClientOptions? options = null)
    {
        var client = new CouchClient(endpoint, credentials, options);
        services.AddSingleton(client);
        return services;
    }
    
    public static IServiceCollection AddCouchDatabase<T>(
      this IServiceCollection services,
      string databaseName)
    {
        services.AddSingleton(provider =>
        {
            var client = provider.GetRequiredService<CouchClient>();
            return client.GetDatabase<T>(databaseName);
        });
        return services;
    }
}