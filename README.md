| Stage      | Status  |
|:-----------|:--------|
| dev        | [![Build status](https://matteobortolazzo.visualstudio.com/CouchDB.NET/_apis/build/status/CI%20-%20Production)](https://matteobortolazzo.visualstudio.com/CouchDB.NET/_build/latest?definitionId=15) |
| master     | [![Build status](https://matteobortolazzo.visualstudio.com/CouchDB.NET/_apis/build/status/CI%20-%20Beta)](https://matteobortolazzo.visualstudio.com/CouchDB.NET/_build/latest?definitionId=16)       |
| Beta       | ![Release Beta status](https://matteobortolazzo.vsrm.visualstudio.com/_apis/public/Release/badge/ff4c14e0-5b2c-4782-b8ad-eb540731c000/4/5)                                                           |
| Production | ![Release Stable status](https://matteobortolazzo.vsrm.visualstudio.com/_apis/public/Release/badge/ff4c14e0-5b2c-4782-b8ad-eb540731c000/3/4)                                                         |


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
* Create a client:
    ```csharp
   using(var client = new CouchClient("http://localhost")) { }
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
| $exists        | o.FieldExists()    |
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
var client = new CouchClient("http://localhost", s => 
    s.UseBasicAuthentication("root", "relax")
)
```

### Cookie authentication

```csharp
var client = new CouchClient("http://localhost", s => s
    .UseCookieAuthentication("root", "relax")
)
```

It's also possible to specify the duration of the session.

```csharp
var client = new CouchClient("http://localhost", s => s
    .UseCookieAuthentication("root", "relax", cookieDuration)
)
```

### Options

The second parameter of the client constructor is a function to configure CouchSettings fluently.

```csharp
var client = new CouchClient("http://localhost", s => s
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

## Advanced

If requests have to be modified before each call, it's possible to override OnBeforeCall.
```csharp
protected virtual void OnBeforeCall(HttpCall call)
```

Also, the constructor accept a ClientFlurlHttpSettings function as third parameter.

```csharp
Action<ClientFlurlHttpSettings> flurlConfigFunc
```

## Contributors

Thanks to [Ben Origas](https://github.com/borigas) for features, ideas and tests like SSL custom validation, multi queryable, async deadlock, cookie authenication and others.