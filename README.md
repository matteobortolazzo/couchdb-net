[![Downloads](https://img.shields.io/nuget/dt/CouchDB.NET.svg)](https://www.nuget.org/packages/CouchDB.NET/)

# CouchDB.NET

A .NET Standard driver for CouchDB.

## LINQ queries

C# query example:

```csharp
var json = _rebels
    .Where(r => 
        r.Surname == "Skywalker" && 
        (
            r.Battles.All(b => b.Planet == "Naboo") ||
            r.Battles.Any(b => b.Planet == "Death Star")
        )
    )
    .OrderByDescending(r => r.Name)
    .ThenByDescending(r => r.Age)
    .Skip(1)
    .Take(2)
    .WithReadQuorum(2)
    .UseBookmark("g1AAAABweJzLY...")
    .WithReadQuorum(150)
    .WithoutIndexUpdate()
    .FromStable()
    .IncludeExecutionStats()
    .IncludeConflicts()
    .Select(r => new {
        r.Name,
        r.Age,
        r.Species
    }).ToList();
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
  "skip": 1,
  "limit": 2,
  "r": 150,
  "bookmark": "g1AAAABweJzLY...",
  "update": false,
  "stable": true,
  "execution_stats":true,
  "conflicts":true,
  "fields": [
    "name",
    "age",
    "species"
  ]
}
``` 

## Getting started

* Install it from NuGet: [https://www.nuget.org/packages/CouchDB.NET](https://www.nuget.org/packages/CouchDB.NET)
* Create a client, where localhost will be the IP address and 5984 is CouchDB standard tcp port:
    ```csharp
   using(var client = new CouchClient("http://localhost:5984")) { }
   ```
* Create a document class:
    ```csharp
    public class Rebel : CouchDocument
    ```
* Get a database reference:
    ```csharp
    var rebels = client.GetDatabase<Rebel>();
    ```
* Query the database
    ```csharp
    var skywalkers = await rebels.Where(r => r.Surname == "Skywalker").ToListAsync();
    ```

## Mango Queries vs LINQ

The database class exposes all the implemented LINQ methods like Where and OrderBy, 
those methods returns an IQueryable.

It's possible to explicitly get the IQueryable calling the AsQueryable() method.

```csharp
var skywalkers =
    from r in rebels.AsQueryable()
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

### IQueryable operations

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

Some methods that are not directly supported by CouchDB are converted to a composition of supported ones,
and the in-memory LINQ method will be executed at the end.

| Input                             | Output                                |
|:----------------------------------|:--------------------------------------|
| Min(r => r.Age)                   | OrderBy(r => r.Age).Take(1)           |
| Max(r => r.Age)                   | OrderByDescending(r => r.Age).Take(1) |
| First()                           | Take(1)								|
| FirstOrDefault()                  | Take(1)								|
| First(r => r.Age == 19)			| Where(r => r.Age == 19).Take(1)       |
| FirstOrDefault(r => r.Age == 19)  | Where(r => r.Age == 19).Take(1)       |
| Single()                          | Take(2)								|
| SingleOrDefault()                 | Take(2)								|
| Single(r => r.Age == 19)			| Where(r => r.Age == 19).Take(2)       |
| SingleOrDefault(r => r.Age == 19) | Where(r => r.Age == 19).Take(2)       |

**WARN**: Do not call a method twice, for example: `Where(func).Single(func)` won't work.

**WARN**: Since Max and Min use **sort**, an *index* must be created.


### All other IQueryables

IQueryable methods that are not natively supported by CouchDB are evaluated in-memory using the IEnumerable counterpart, if possible.

For example: `All` `Any` `Avg` `Count` `DefaultIfEmpty` `ElementAt` `ElementAtOrDefault` `GroupBy` `Last` `Reverse` `SelectMany` `Sum`


## Client operations

```csharp
// CRUD from class name (rebels)
var rebels = client.GetDatabase<Rebel>();
var rebels = await client.CreateDatabaseAsync<Rebel>();
await client.DeleteDatabaseAsync<Rebel>();
// CRUD specific name
var rebels = client.GetDatabase<Rebel>("naboo_rebels");
var rebels = await client.CreateDatabaseAsync<Rebel>("naboo_rebels");
await client.DeleteDatabaseAsync<Rebel>("naboo_rebels");
// Utils
var isRunning = await client.IsUpAsync();
var databases = await client.GetDatabasesNamesAsync();
var tasks = await client.GetActiveTasksAsync();
```

## Database operations

```csharp
// CRUD
await rebels.CreateAsync(rebel);
await rebels.CreateOrUpdateAsync(rebel);
await rebels.DeleteAsync(rebel);
var rebel = await rebels.FindAsync(id);
var rebel = await rebels.FindAsync(id, withConflicts: true);
var list = await rebels.FindManyAsync(ids);
var list = await rebels.QueryAsync(someMangoJson);
var list = await rebels.QueryAsync(someMangoObject);
// Bulk
await rebels.CreateOrUpdateRangeAsync(moreRebels);
// Utils
await rebels.CompactAsync();
var info = await rebels.GetInfoAsync();
// Security
await rebels.Security.SetInfoAsync(securityInfo);
var securityInfo = await rebels.Security.GetInfoAsync();
```

## Authentication

If authentication is needed currently there are two ways: Basic and Cookie authentication.

### Basic authentication

```csharp
var client = new CouchClient("http://localhost:5984", s => 
    s.UseBasicAuthentication("root", "relax")
)
```

### Cookie authentication

```csharp
var client = new CouchClient("http://localhost:5984", s => s
    .UseCookieAuthentication("root", "relax")
)
```

It's also possible to specify the duration of the session.

```csharp
var client = new CouchClient("http://localhost:5984", s => s
    .UseCookieAuthentication("root", "relax", cookieDuration)
)
```

### Proxy authentication

```csharp
var client = new CouchClient("http://localhost:5984", s => s
  .UseProxyAuthentication("root", new[] { "role1", "role2" })
```

### Options

The second parameter of the client constructor is a function to configure CouchSettings fluently.

```csharp
var client = new CouchClient("http://localhost:5984", s => s
    .UseBasicAuthentication("root", "relax")
    .DisableEntitisPluralization()
    ....
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
| EnsureDatabaseExists           | If a database doesn't exists, it creates it.  |
| DisableLogOutOnDispose         | Disables log out on client dispose.           | 

- **DocumentCaseTypes**: None, UnderscoreCase *(default)*, DashCase, KebabCase.
- **PropertyCaseTypes**: None, CamelCase *(default)*, PascalCase, UnderscoreCase, DashCase, KebabCase.

### Bookmark and Execution stats

If bookmark and execution stats must be retrived, call *ToCouchList* or *ToCouchListAsync*.

```csharp
var allRebels = rebels.ToCouchList();
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

## Advanced

If requests have to be modified before each call, it's possible to override OnBeforeCallAsync.
```csharp
protected virtual Task OnBeforeCallAsync(HttpCall call)
```

Also, the constructor accept a ClientFlurlHttpSettings function as third parameter.

```csharp
Action<ClientFlurlHttpSettings> flurlConfigFunc
```

## Contributors

Thanks to [Ben Origas](https://github.com/borigas) for features, ideas and tests like SSL custom validation, multi queryable, async deadlock, cookie authenication and many others.
