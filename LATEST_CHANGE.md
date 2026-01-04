# 4.0.0

V4 is almost a complete rewrite of the library. 

In the effort to migrate away from `Newtonsoft.Json` in favor of `System.Text.Json`, I found myself rewriting large portions of the library and then thinking if the structure made sense anymore.

The answer was no. Up to this point, the library was inspired by `Entity Framework`, but it made no sense given there was no tracking of changes. 
So for this version, I decided a `Cosmos SDK` approach would suit better.

Finally, I chose a zero-dependency approach, removing `Flurl` and `Humanizer` dependencies.
Hence, all the following breaking changes.

## Breaking Changes

### General

* Removed Dependency Injection packages
* Removed `CouchContext`
* Removed `Newtonsoft.Json` dependency in favor of `System.Text.Json`
* Removed `Flurl` dependency
* Removed `Humanizer` dependency
* Remove `Couch` prefix from most classes
* Removed `CouchDocument`, any class/record can now be used (Id and Rev are special and handled automatically)
* CRUD operations accept dedicate options object instead of parameters
* Internal serialization is done via source-generated serializers for better performance, except for objects that deal with user-provided types (like query results)

### Client

* Changed `CouchClient` constructor parameters
* Removed `OptionsBuider`
* Renamed `CouchOptions` to `CouchClientOptions`
* Authentication is now via `CouchCredentials` implementations
* `CouchOptions` has now only four properties and the others are not relevant anymore

### Database

* Removed reflection+humanizer-based naming conventions in favor of explicit names. Related methods are removed.
* Removed database splitting (might be reintroduced later)
* Renamed `Database.FindAsync` to `Database.ReadItemAsync`
* Renamed `Database.AddAsync` to `Database.CreateItemAsync`
* Renamed `Database.RemoveAsync` to `Database.DeleteItemAsync`
* GetDetailedViewAsync
* Replaced `AddOrUpdateRangeAsync` and `DeleteRangeAsync` with `ExecuteBulkItemOperationsAsync`
* Added `Database.UpdateItemAsync`
* Added `Database.UpsertAttachmentAsync`
* Added `Database.DeleteAttachmentAsync`
* Removed `Database.AddOrUpdateAsync`
* Removed `Database.GetDetailedViewAsync` as `GetViewAsync` now returns all metadata
* Changed `Find/ReadItemAsync` response to return a `ReadItemResponse<T>` to split the document from the metadata

### Query

* `ThrowOnQueryWarning` now defaults to `true` (use new `With/WithoutQueryParam` LINQ methods)
* Removed `ToCouchQueryAsync`, replaced by `ToListAsync`
* Changed `ToListAsync` to return a `CouchList<T>` instead of a `List<T>`

### Attachments

* Now returned in `ReadItemResponse<T>`
* To add them during document creation, use `CreateItemRequestOptions`
* To manage them use `Upsert/DeleteAttachmentAsync`

### Others

* Removed `ChangeUserPassword` as it's just an update