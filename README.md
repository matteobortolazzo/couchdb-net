[![Downloads](https://img.shields.io/nuget/dt/CouchDB.NET.svg)](https://www.nuget.org/packages/CouchDB.NET/)

# CouchDB.NET

EF Core-like CouchDB experience for .NET!

## LINQ queries

C# query example:

```csharp
// Setup
public class MyDeathStarContext : CouchContext
{
    public CouchDatabase<Rebel> Rebels { get; set; }
    public CouchDatabase<Clone> Clones { get; set; }

    protected override void OnConfiguring(CouchOptionsBuilder optionsBuilder)
    {
      optionsBuilder
        .UseEndpoint("http://localhost:5984/")
        .EnsureDatabaseExists()
        .UseBasicAuthentication(username: "anakin", password: "empirerule");
    }
}

// Usage
await using var context = new MyDeathStarContext();
var skywalkers = await context.Rebels
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
    .Select(r => new {
        r.Name,
        r.Age
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
    { "name": "desc" },
    { "age": "desc" }
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
  * [Native method](#native-method)
  * [Composite methods](#composite-methods)
* [Client operations](#client-operations)
* [Database operations](#database-operations)
* [Authentication](#authentication)
* [Options](#options)
* [Custom JSON values](#custom-json-values)
* [Attachments](#attachments)
* [DB Changes Feed](#db-changes-feed)
  * [Feed Options](#feed-options)
  * [Feed Filter](#feed-filter)
* [Local (non-replicating) Documents](#local-(non-replicating)-documents)
* [Bookmark and Execution stats](#bookmark-and-execution-stats)
* [Users](#users)
* [Dependency Injection](#dependency-injection)
* [Advanced](#advanced)
* [Contributors](#contributors)

## Getting started

* Install it from NuGet: [https://www.nuget.org/packages/CouchDB.NET](https://www.nuget.org/packages/CouchDB.NET)
* Create a context or a client, where localhost will be the IP address and 5984 is CouchDB standard tcp port:
  ```csharp
  await using var context = new MyDeathStarContext(builder => {});
  // or
  await using(var client = new CouchClient("http://localhost:5984", builder => {})) { }
  ```
* Create a document class:
  ```csharp
  public class Rebel : CouchDocument
  ```
* Get a database reference:
  ```csharp
  var rebels = context.Rebels;
  // or
  var rebels = client.GetDatabase<Rebel>();
  ```
* Query the database
  ```csharp
  var skywalkers = await rebels.Where(r => r.Surname == "Skywalker").ToListAsync();
  ```

## Queries

The database class exposes all the implemented LINQ methods like Where and OrderBy, 
those methods returns an IQueryable.

LINQ are supported natively to the following is possible:

```csharp
var skywalkers =
    from r in context.Rebels
    where r.Surname == "Skywalker"
    select r;
```

### Selector

The selector is created when the method Where (IQueryable) is called.
If the Where method is not called in the expression, it will at an empty selector.

#### Combinations

| Mango      |      C#          |
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
| fields          | Select(x => new { })                                 |
| use_index       | UseIndex("design_document")                          |
| use_index       | UseIndex(new [] { "design_document", "index_name" }) |
| r               | WithReadQuorum(n)                                    |
| bookmark        | UseBookmark(s)                                       |
| update          | WithoutIndexUpdate()                                 |
| stable          | FromStable()                                         |
| execution_stats | IncludeExecutionStats()                              |
| conflicts       | IncludeConflicts()                                   |

### Composite methods

Some methods that are not directly supported by CouchDB are converted to a composition of supported ones!

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


### All other IQueryables methods

Since v2.0 `IQueryable` methods that are not natively supported will throw an exception.

## Client operations

```csharp
// CRUD from class name (rebels)
var rebels = client.GetDatabase<Rebel>();
var rebels = await client.GetOrCreateDatabaseAsync<Rebel>();
var rebels = await client.CreateDatabaseAsync<Rebel>();
await client.DeleteDatabaseAsync<Rebel>();
// CRUD specific name
var rebels = client.GetDatabase<Rebel>("naboo_rebels");
var rebels = client.GetOrCreateDatabaseAsync<Rebel>("naboo_rebels");
var rebels = await client.CreateDatabaseAsync<Rebel>("naboo_rebels");
await client.DeleteDatabaseAsync("naboo_rebels");
// Utils
var isRunning = await client.IsUpAsync();
var databases = await client.GetDatabasesNamesAsync();
var tasks = await client.GetActiveTasksAsync();
```

## Database operations

```csharp
// CRUD
await rebels.AddAsync(rebel);
await rebels.AddOrUpdateAsync(rebel);
await rebels.RemoveAsync(rebel);
var rebel = await rebels.FindAsync(id);
var rebel = await rebels.FindAsync(id, withConflicts: true);
var list = await rebels.FindManyAsync(ids);
var list = await rebels.QueryAsync(someMangoJson);
var list = await rebels.QueryAsync(someMangoObject);
// Bulk
await rebels.AddOrUpdateRangeAsync(moreRebels);
// Utils
await rebels.CompactAsync();
var info = await rebels.GetInfoAsync();
// Security
await rebels.Security.SetInfoAsync(securityInfo);
var securityInfo = await rebels.Security.GetInfoAsync();
```

## Authentication

```csharp
// Basic
.UseBasicAuthentication("root", "relax")

// Cookie
.UseCookieAuthentication("root", "relax")
.UseCookieAuthentication("root", "relax", cookieDuration)

// Proxy
.UseProxyAuthentication("root", new[] { "role1", "role2" })

// JTW
.UseJwtAuthentication("token")
.UseJwtAuthentication(async () => await NewTokenAsync())
```

### Options

The second parameter of the client constructor is a function to configure CouchSettings fluently.

```csharp
public class MyDeathStarContext : CouchContext
{
  /* ... */

    protected override void OnConfiguring(CouchOptionsBuilder optionsBuilder)
    {
      optionsBuilder
        .UseBasicAuthentication("root", "relax")
        .DisableEntitisPluralization()
        ...
    }
}

// or

var client = new CouchClient("http://localhost:5984", builder => builder
    .UseBasicAuthentication("root", "relax")
    .DisableEntitisPluralization()
    ...
)
```
| Method                         | Description                                   |
|:-------------------------------|:----------------------------------------------|
| UseBasicAuthentication         | Enables basic authentication.                 |
| UseCookieAuthentication        | Enables cookie authentication.                |
| IgnoreCertificateValidation    | Removes any SSL certificate validation.       |
| ConfigureCertificateValidation | Sets a custom SSL validation rule.            |
| DisableDocumentPluralization   | Disables documents pluralization in requests. |
| SetDocumentCase                | Sets the format case for documents.           |
| SetPropertyCase                | Sets the format case for properties.          |
| SetNullValueHandling           | Sets how to handle null values.               |
| DisableLogOutOnDispose         | Disables log out on client dispose.           | 

- **DocumentCaseTypes**: None, UnderscoreCase *(default)*, DashCase, KebabCase.
- **PropertyCaseTypes**: None, CamelCase *(default)*, PascalCase, UnderscoreCase, DashCase, KebabCase.

## Custom JSON values

If you need custom values for documents and properties, it's possible to use JsonObject and JsonProperty attributes.

```csharp
[JsonObject("custom-rebels")]
public class OtherRebel : Rebel

[JsonProperty("rebel_bith_date")]
public DateTime BirthDate { get; set; }
```

## Attachments

The driver fully support attachments, you can list, create, delete and download them.

```csharp
// Get a document
var luke = new Rebel { Name = "Luke", Age = 19 };

// Add in memory
var pathToDocument = @".\luke.txt"
luke.Attachments.AddOrUpdate(pathToDocument, MediaTypeNames.Text.Plain);

// Delete in memory
luke.Attachments.Delete("yoda.txt");

// Save
luke = await rebels.CreateOrUpdateAsync(luke);

// Get
CouchAttachment lukeTxt = luke.Attachments["luke.txt"];

// Iterate
foreach (CouchAttachment attachment in luke.Attachments)
{ 
  ...
}

// Download
string downloadFilePath = await rebels.DownloadAttachment(attachment, downloadFolderPath, "luke-downloaded.txt");
```

## DB Changes Feed

The following *feed modes* are supported: `normal`, `longpool` and `continuous`.
Also all *options* and *filter types* are supported.

`Continuous mode` is probably the most useful and it's implemented with the new `IAsyncEnumerable`.

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

// Use
ChangesFeedResponse<Rebel> changes = await GetChangesAsync(options: null, filter);
```

## Local (non-replicating) Documents

The Local (non-replicating) document interface allows you to create local documents that are not replicated to other databases.

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
settings = await _rebels.LocalDocuments.GetAsync<RebelSettings>(docId);

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

If bookmark and execution stats must be retrived, call *ToCouchList* or *ToCouchListAsync*.

```csharp
var allRebels = await rebels.ToCouchListAsync();

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

## Dependency Injection

* Install the DI package from NuGet: [https://www.nuget.org/packages/CouchDB.NET.DependencyInjection](https://www.nuget.org/packages/CouchDB.NET.DependencyInjection)
* Create a `CouchContext` with a constructor like the following:

```csharp
public class MyDeathStarContext : CouchContext
{
    public CouchDatabase<Rebel> Rebels { get; set; }

    public MyDeathStarContext(CouchOptions<MyDeathStarContext> options)
        : base(options) { }
}
```

* In the `Startup` class register the context:

```csharp
// ConfigureServices
services.AddCouchContext<MyDeathStarContext>(builder => builder
    .UseEndpoint("http://localhost:5984")
    .UseBasicAuthentication(username: "admin", password: "admin"));
```

* Inject the context:

```csharp
// RebelsController
public class RebelsController : Controller
{
    private readonly MyDeathStarContext _context;

    public RebelsController(MyDeathStarContext context)
    {
        _context = context;
    }
}
```

**Info:** The context is registered as a `singleton`.

## Advanced

If requests have to be modified before each call, it's possible to override OnBeforeCallAsync.

```csharp
protected virtual Task OnBeforeCallAsync(HttpCall call)
```

Also, the configurator has `ConfigureFlurlClient` to set custom HTTP client options.

## Contributors

Thanks to [Ben Origas](https://github.com/borigas) for features, ideas and tests like SSL custom validation, multi queryable, async deadlock, cookie authenication and many others.

Thanks to [n9](https://github.com/n9) for proxy authentication, some bug fixes, suggestions and the great feedback on the changes feed feature!

Thanks to [Marc](https://github.com/bender-ristone) for NullValueHandling, bug fixes and suggestions!
