# CouchDB.NET

A .NET driver for CouchDB. * **Still in development** *

## LINQ-like queries

C# query example:

```csharp
var houses = await housesDb.Documents
    .Where(h => 
        h.Owner.Name == "Bobby" && 
        (h.Floors.All(f => f.Area < 120) || h.Floors.Any(f => f.Area > 500))
    )
    .OrderByDescending(h => h.Owner.Name)
    .ThenByDescending(h => h.ConstructionDate)
    .Skip(0)
    .Take(50)
    .Select(
        h => h.Owner.Name, 
        h => h.Address,
        h => h.ConstructionDate)
    .UseBookmark("g1AAAABweJzLY...")
    .WithReadQuorum(150)
    .UpdateIndex()
    .FromStable()
    .ToListAsync();
```

The produced Mango JSON:
```json
{
    "selector":{ 
        "owner.name":"Bobby",
	"$or":[
    	    {"floors":{"$allMatch":{"area":{"$lt":120}}}},
            {"floors":{"$elemMatch":{"area":{"$gt":500}}}}
    	]
    },
    "limit":50,
    "skip":0,
    "sort":[
    	{"owner.name":"desc"},
    	{"construction_date":"desc"}
    ],
    "fields":[
	    "owner.name",
	    "address",
	    "construction_date"
        ],
    "bookmark":"g1AAAABweJzLY...",
    "r":150,
    "update":true,
    "stable":true
}
``` 

## Getting started

* Install it from NuGet: https://www.nuget.org/packages/CouchDB.NET
* Create a client:
    ```csharp
    var client = new CouchClient("http://127.0.0.1:5984");
   ```
* If authentication needed:
    ```csharp
    client.ConfigureAuthentication("myusername", "mypassword");
    ```
* Extend **CouchEntity** in every model class:
    ```csharp
    public class House : CouchEntity
    ```
* Use the **JsonProperty** attribute if you need to override default names:
    ```csharp
    [JsonProperty("construction_date")]
    public DateTime ConstructionDate { get; set; }
    ```

## Database operations
```csharp
var allDbs = await client.GetDatabasesNamesAsync();
var info = await client.GetDatabaseInfoAsync("dbName");
var db = await client.GetDatabaseAsync<House>("dbName");

await client.AddDatabaseAsync("dbName");
await client.RemoveDatabaseAsync("dbName");
```

## Documents operations
```csharp
// CRUD
await housesDb.Documents.AddAsync(house);
await housesDb.Documents.UpdateAsync(house);
await housesDb.Documents.RemoveAsync(house);
var house = await housesDb.Documents.FindAsync(houseId);
// Bulk
await housesDb.Documents.AddRangeAsync(houses);
await housesDb.Documents.UpdateRangeAsync(houses);
var houses = await housesDb.Documents.FindAsync(houseId1, houseId2, houseId3);
var houses = await housesDb.Documents.FindAsync(houseIdArray);
// Enebles stats in queries
housesDb.Documents.EnableStats();
var stats = housesDb.Documents.LastExecutionStats;
// Bookmakrs
var bookmark = housesDb.Documents.LastBookmark;
// Find
var allHouses = await housesDb.Documents.ToListAsync();
var bobbysOnes = await housesDb.Documents.Where(h => h.Owner.Name == "Bobby").ToListAsync();
```

## Indexes (WIP)

C# example:
```csharp
await housesDb.NewIndexAsync(s => 
        s.Descending(h => h.Address)
            .ThenDescending(h => h.ConstructionDate), 
        name: "useless_index",
        designDocumentName: "my_design"
    );
```
To JSON:
```json
{
    "index":{
        "fields":[
            {"address":"desc"},
            {"construction_date":"desc"}
        ]
    },
    "name":"useless_index",
    "ddoc":"my_design",
    "type":"json"
}
```
