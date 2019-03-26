# CouchDB.NET
## A .NET Standard driver for CouchDB.

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
   using(var client = new CouchClient("http://localhost:5984")) { }
   ```
* Create an entity class:
    ```csharp
    public class Rebel : CouchEntity
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
| $in            | a.In(list)         |
| $nin           | !a.In(list)        |
| $size          | a.Count == x       |
| $mod           | n % x = y          |
| $regex         | s.IsMatch(rx)      |

### IQueryable operations

| Mango     | C#                                                   |
|:----------|:-----------------------------------------------------|
| limit     | Take(n)                                              |
| skip      | Skip(n)                                              |
| sort      | OrderBy(..)                                          |
| sort      | OrderBy(..).ThenBy()                                 |
| sort      | OrderByDescending(..)                                |
| sort      | OrderByDescending(..).ThenByDescending()             |
| fields    | Select(x => new { })                                 |
| use_index | UseIndex("design_document")                          |
| use_index | UseIndex(new [] { "design_document", "index_name" }) |
| r         | WithReadQuorum(n)                                    |
| bookmark  | UseBookmark(s)                                       |
| update    | WithoutIndexUpdate()                                 |
| stable    | FromStable()                                         |

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
// Bulk
await rebels.CreateOrUpdateRangeAsync(moreRebels);
// Utils
await rebels.CompactAsync();
var info = await rebels.GetInfoAsync();
```

## Authentication

If authentication is needed currently there are two ways: Basic and Cookie authentication.

### Basic authentication

```csharp
var client = new CouchClient("http://localhost:5984", s => 
    s.ConfigureBasicAuthentication("root", "relax")
)
```

### Cookie authentication

```csharp
var client = new CouchClient("http://localhost:5984", s => s
    .ConfigureCookieAuthentication("root", "relax")
)
```

It's also possible to specify the duration of the session.

```csharp
var client = new CouchClient("http://localhost:5984", s => s
    .ConfigureCookieAuthentication("root", "relax", cookieDuration)
)
```

### Options

The second parameter of the client constructor is a function to configure CouchSettings fluently.

```csharp
var client = new CouchClient("http://localhost:5984", s => s
    .ConfigureBasicAuthentication("root", "relax")
    .DisableEntitisPluralization()
    ....
)
```
| Method                         | Description                                  |
|:-------------------------------|:---------------------------------------------|
| ConfigureBasicAuthentication   | Enables basic authentication.                |
| ConfigureCookieAuthentication  | Enables cookie authentication.               |
| IgnoreCertificateValidation    | Removes any SSL certificate validation.      |
| ConfigureCertificateValidation | Sets a custom SSL validation rule.           |
| DisableEntitisPluralization    | Disables entities pluralization in requests. |
| SetEntityCase                  | Sets the format case for entities.           |
| SetPropertyCase                | Sets the format case for properties.         |
| EnsureDatabaseExists           | If a database doesn't exists, it creates it. |

- **EntityCaseTypes**: None, UnderscoreCase *(default)*, DashCase, KebabCase.
- **PropertyCaseTypes**: None, CamelCase *(default)*, PascalCase, UnderscoreCase, DashCase, KebabCase.

## Custom JSON values

If you need custom values for entities and properties, it's possible to use JsonObject and JsonProperty attributes.

```csharp
[JsonObject("custom-rebels")]
public class OtherRebel : Rebel

[JsonProperty("rebel_bith_date")]
public DateTime BirthDate { get; set; }
```

## Advanced

If requests have to be updated it's possible to override OnBeforeCall.
```csharp
protected virtual void OnBeforeCall(HttpCall call)
```

Also, the constructor accept a ClientFlurlHttpSettings function as third parameter.

```csharp
Action<ClientFlurlHttpSettings> flurlConfigFunc
```

## Contributors

Thanks to [Ben Origas](https://github.com/borigas) for features, ideas and tests like SSL custom validation, multi queryable, async deadlock, cookie authenication and others.