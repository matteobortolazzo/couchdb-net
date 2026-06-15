# 4.1.0 (2026-06-15)

## Bugs

* **Find queries**: `_id` and `_rev` fields from CouchDB `_find` responses are now correctly mapped to `Id` and `Rev` properties on result documents. Previously, these fields were silently dropped during deserialization. ([#219](https://github.com/matteobortolazzo/couchdb-net/pull/219), based on [#218](https://github.com/matteobortolazzo/couchdb-net/pull/218)) Thanks [@gchiappe](https://github.com/gchiappe)
* **Active tasks**: Fixed `GetActiveTasksAsync()` throwing `InvalidOperationException` at runtime due to `UnixTimestampSecondsConverter` type mismatch with `ActiveTask.StartedOn`/`UpdatedOn` properties ([#219](https://github.com/matteobortolazzo/couchdb-net/pull/219))
* **Changes feed**: Fixed `ReceiveStream()` prematurely disposing the HTTP response content stream when using POST-based continuous change feed filters ([#219](https://github.com/matteobortolazzo/couchdb-net/pull/219))

## Improvements

* Extracted shared `DocumentRewriter` for `_id`/`_rev` property rewriting, used by both `FindResultConverter` and `ReadItemResponseConverter`

# 4.0.0 (2026-01-03)

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

# 3.7.0 (2025-12-01)

## Features

* **Partitions**: Add comprehensive support for partitions ([#213](https://github.com/matteobortolazzo/couchdb-net/pull/213))
* **Get/SetRevisionLimit**: Added method to get the revision limit of a database ([#202](https://github.com/matteobortolazzo/couchdb-net/pull/202))
* **ThrowOnQueryWarning**: Added option to throw exception on query warnings ([#205](https://github.com/matteobortolazzo/couchdb-net/pull/205))

## Bugs

* **IncludeExecutionStats**: Fixed deserialization exception ([#204](https://github.com/matteobortolazzo/couchdb-net/pull/204))
* **Local Docs**: Fixed ID encoding ([#206](https://github.com/matteobortolazzo/couchdb-net/pull/206))

# 3.6.1 (2024-04-23)

## Bugs

* **Change feed**: Fixed an issue causing an endless change notification for all documents under certain conditions ([#200](https://github.com/matteobortolazzo/couchdb-net/pull/201))

# 3.6.0 (2024-03-11)

## Bugs

* **Change feed**: Fixed automatic resume at last change in continuous feed ([#198](https://github.com/matteobortolazzo/couchdb-net/issues/198))

# 3.5.0 (2024-02-03)

## Features

* **Find**: Added support for fetching attachments with entire content ([#194](https://github.com/matteobortolazzo/couchdb-net/issues/194))

# 3.4.0 (2023-06-21)

## Features

* **Database split**: Configurable field for document discrimination ([#150](https://github.com/matteobortolazzo/couchdb-net/issues/150))
* **Find**: Added all options and responses ([#182](https://github.com/matteobortolazzo/couchdb-net/issues/182))
* **Change feed**: Adds support for database split ([#187](https://github.com/matteobortolazzo/couchdb-net/issues/187))
* **Replicas**: Adds `CreateTarget` option ([#189](https://github.com/matteobortolazzo/couchdb-net/issues/189))

## Bugs

* **Queries**: Fix when `In` is called inside `Any` ([#183](https://github.com/matteobortolazzo/couchdb-net/issues/183))
* **Database split**: Fix `FirstOrDefault` without filter queries ([#185](https://github.com/matteobortolazzo/couchdb-net/issues/185))

# 3.3.1 (2022-10-26)

## Bug Fixes

* **Dependency Injection**: Fix dependency injection packages references ([#180](https://github.com/matteobortolazzo/couchdb-net/pull/180))

# 3.3.0 (2022-10-20)

## Features

* **Bulk Delete**: Adds support to replication ([#171](https://github.com/matteobortolazzo/couchdb-net/issues/171))
* **Revision Support**: Support for revisions in add and update ([#170](https://github.com/matteobortolazzo/couchdb-net/pull/170))
* **Deleted Flag**: Added deleted flag on document ([#154](https://github.com/matteobortolazzo/couchdb-net/pull/154))

## Bug Fixes

* **Replication**: Added replication methods in `ICouchDatabase` interface ([#173](https://github.com/matteobortolazzo/couchdb-net/pull/173))
* **Document ID**: Support IDs with special characters ([#172](https://github.com/matteobortolazzo/couchdb-net/pull/172))
* 
# 3.2.0 (2022-07-03)

## Features

* **Replication**: Adds support to replication ([#151](https://github.com/matteobortolazzo/couchdb-net/pull/151))
* **Attachments**: Adds DownloadAttachmentAsStreamAsync ([#152](https://github.com/matteobortolazzo/couchdb-net/pull/152))
* **IsMatch**: Support multiline regex ([#161](https://github.com/matteobortolazzo/couchdb-net/pull/161))

## Bug Fixes

* **ElementAt**: Fixes query on .NET 6. ([#156](https://github.com/matteobortolazzo/couchdb-net/pull/156))
* **Attachments**: Fixes attachments in FindAsync. ([#159](https://github.com/matteobortolazzo/couchdb-net/pull/159))
* **Attachments**: Fixes attachments uploads ([#159](https://github.com/matteobortolazzo/couchdb-net/pull/159))
* **Attachments**: Fixes Bad Request on attachment upload. ([#164](https://github.com/matteobortolazzo/couchdb-net/pull/164))
* **GetInfoAsync**: Fixed 32-bit integer overflow. ([#165](https://github.com/matteobortolazzo/couchdb-net/pull/165))

# 3.1.1 (2021-10-14)

## Bug Fixes

* **Query**: Fix First/Last with conditions fail. ([#142](https://github.com/matteobortolazzo/couchdb-net/issues/142))
* **Query**: Fix First/Last on splitted database. ([#136](https://github.com/matteobortolazzo/couchdb-net/issues/136))
* **Query**: Throws exception on List.Count instead of wrong query. ([#138](https://github.com/matteobortolazzo/couchdb-net/issues/138))
* **Query**: Fix multi thread call issues. ([#133](https://github.com/matteobortolazzo/couchdb-net/issues/133))
* **FindManyAsync**: Filters out null results. ([#141](https://github.com/matteobortolazzo/couchdb-net/issues/141)) Thanks [AlexandrSHad](https://github.com/AlexandrSHad)
* **Continuous Changes**: Fix multi thread issues. ([#140](https://github.com/matteobortolazzo/couchdb-net/issues/140))

# 3.1.0 (2020-03-20)

## Features

* **Views**: Possibility to query multiple views at once. ([#126](https://github.com/matteobortolazzo/couchdb-net/issues/126)) Thanks [Panos](https://github.com/panoukos41)
* **Partitioned database**: It's possible to create partitioned databases. ([#122](https://github.com/matteobortolazzo/couchdb-net/issues/122))

## Bug Fixes

* **Views**: `CouchViewOptions` are serialized correctly when overriding the serializer. ([#125](https://github.com/matteobortolazzo/couchdb-net/issues/125)) Thanks [Panos](https://github.com/panoukos41)
* **PropertyCaseType**: `PropertyCaseType` is not applied on internal properties anymore. ([#127](https://github.com/matteobortolazzo/couchdb-net/issues/127))

# 3.0.1 (2020-03-10)

## Bug Fixes
* **Table Splitting**: Fix discriminator. ([#120](https://github.com/matteobortolazzo/couchdb-net/issues/120))

# 3.0.0 (2020-03-09)

## Breaking Changes
* Update to [Flurl 3](https://github.com/tmenier/Flurl/releases/tag/Flurl.Http.3.0.0). There should be no differences for the end user, but keep in mind.

## Features
* **Table Splitting**: Ability to use the same database for different document with automatic filtering. ([#106](https://github.com/matteobortolazzo/couchdb-net/issues/106))
* **Views**: Ability to get views. Thanks to [panoukos41](https://github.com/panoukos41)

## Improvements
* **Logical Expressions Prune**: If expressions are constant booleans, they are removed automatically keeping the query valid. ([#113](https://github.com/matteobortolazzo/couchdb-net/issues/113))
* **IsUpAsync**: Returns false on timeout and on not successful codes. ([#107](https://github.com/matteobortolazzo/couchdb-net/issues/107))
* **FindAsync**: Faster when document is not found. ([#92](https://github.com/matteobortolazzo/couchdb-net/issues/92))

# 2.1.0 (2020-09-19)

## Features
* **Indexes"**: Ability to create indexes. ([#102](https://github.com/matteobortolazzo/couchdb-net/issues/102))
* **Null values"**: New `SetNullValueHandling` method for `CouchOptionsBuilder` to set how to handle null values. ([#101](https://github.com/matteobortolazzo/couchdb-net/issues/101))
* **Query"**: New `Select` and `Convert` methods to select specific fields.

## Bug Fixes
* **Conflicts**: Fix the query parameter value to get conflicts. ([#100](https://github.com/matteobortolazzo/couchdb-net/issues/100))
* **Query**: Fix queries when variables are used. ([#104](https://github.com/matteobortolazzo/couchdb-net/issues/104))

# 2.0.2 (2020-07-18)

## Features
* **Users"**: Added `ChangeUserPassword` method for `ICouchDatabase<CouchUser>`.

## Bug Fixes
* **IsMatch**: Back to public instead of internal;
* **AddOrUpdate**: Added `Async` postfix.

# 2.0.0 (2020-07-15)

## Improvements
* **Queries"**: Complete rewrite. `Async/await` operations are supported natively and so are `CancellationTokens`;
* **Queries"**: Support for multiple `Where` calls;
* **Queries"**: Support for `Min`, `Max`, `Sum`, `Average`, `Any`, `All`, `Last` methods (async);
* **Changes Feed:** Support for realtime document changes with `IAsyncEnumerable`;
* **Authentication:** Support for `Proxy` and `JTW` authentication;
* **CouchDatabase:**: `CouchDatabase` now implements `IQueryable`;
* **CouchDatabase:**: `NewRequest` method exposed;
* **CouchContext:** New `CouchContext` class to have an experience similar to *EF Core*;
* **Dependency Injection:** New NuGet package to help with DI;
* **Local Documents:** New `LocalDocuments` property in `CouchDatabase`;
* **Generic:** `ICouchClient` and `ICouchDatabase` interfaces introduced;
* **Generic:** `async` methods support `CancellationTokens`;
* **Builds:** Build definition move to YAML files.

## Breaking Changes
* **Settings:** `CouchSettings` replaced with `CouchOptions` and `CouchOptionsBuilder`;
* **Queries:** Methods that cannot be converted to queries throw exceptions;
* **CouchDatabase:** `GetDatabase` doesn't create the DB if not found anymore. Use `GetOrCreateDatabaseAsync` instead;
* **CouchDatabase:** Create, CreateOrUpdate and Delete documents are renamed to Add, AddOrUpdate and Remove.

## Bug Fixes
* **FindMany**: Fix crash when document does not exist.

# 1.2.2 (2020-07-02)

## Bug Fix
* **JSON content:** Fix issue with *purge_seq* from into to string.

# 1.2.1 (2020-02-25)

## Bug Fix
* **JSON content:** Fix issue with JSON content as a value. ([#PR59](https://github.com/matteobortolazzo/couchdb-net/pull/59))

# 1.2.0 (2020-01-24)

## Features
* **Attachments:** Adds support for attachments. ([#PR56](https://github.com/matteobortolazzo/couchdb-net/pull/56))

# 1.1.5 (2019-12-19)

## Bug Fixes
* **Database:** Fixing special characters escaping in databases names. ([#PR54](https://github.com/matteobortolazzo/couchdb-net/pull/54))

# 1.1.4 (2019-08-19)
## Bug Fixes
* **Queries:** Fixing enums serialized as string instead of int bug. ([#PR49](https://github.com/matteobortolazzo/couchdb-net/pull/49))

# 1.1.3 (2019-06-14)

## Bug Fixes
* **Exception:** Fixing null reference exception and poor exception handling. ([#PR45](https://github.com/matteobortolazzo/couchdb-net/pull/45))

# 1.1.2 (2019-06-08)

## Bug Fixes
* **Client:** Prevent deadlocks when run against .NET Framework. ([#PR43](https://github.com/matteobortolazzo/couchdb-net/pull/43))

# 1.1.1 (2019-06-02)

## Features
* **Single/SingleOrDefault:** Methods implementated as composite supported methods (Where and Take(2)).

## Bug Fixes
* **Queries:** Implicit bools in nested methods. ([#PR41](https://github.com/matteobortolazzo/couchdb-net/pull/41))
* **FxCopAnalyzers:** Removed from NuGet dependencies.

# 1.1.0 (2019-05-05)

## Features
* **_find:** IQueryable methods that are not supported by CouchDB are evaluated in-memory using the IEnumerable counterpart, if possible.

# 1.0.2 (2019-05-02)

## Bug Fixes
* **_find:** Boolean member expressions converted to binary expressions in Where (Fix [#PR36](https://github.com/matteobortolazzo/couchdb-net/pull/36)).

# 1.0.1 (2019-04-27)

## Bug Fixes
* **Everywhere:** Flurl JSON serialization based on CouchSettings' PropertyCaseType.

# 1.0.1-beta.4 (2019-04-25)

## Features
* **CouchClient:** add FindManyAsync(ids) ([#PR33](https://github.com/matteobortolazzo/couchdb-net/pull/33)).
* **CouchClient:** add QueryAsync(someMangoJson) ([#PR32](https://github.com/matteobortolazzo/couchdb-net/pull/32)).
* **CouchClient:** add QueryAsync(someMangoObject) ([b4dd1b2](https://github.com/matteobortolazzo/couchdb-net/commit/b4dd1b2)).

## Bug Fixes
* **_find:** removed T : IComparable from In() and Contains() methods ([#PR31](https://github.com/matteobortolazzo/couchdb-net/pull/31)).
* **_find:** fix single element array queries. ([#PR34](https://github.com/matteobortolazzo/couchdb-net/pull/34)).

# 1.0.1-beta.3 (2019-04-03)

## Breaking Changes
* **_find:** from r.prop.FieldExists() to r.FieldExists("someprop").

## Features
* **CouchClient:** implements protected virtual void Dispose(bool disposing).

## Improvements
* **Global:** FxCop analizers add to the project.

# 1.0.1-beta.2 (2019-04-03)

## Bug Fixes
* **_find:** Guid support, and all other constants  ([#PR26](https://github.com/matteobortolazzo/couchdb-net/pull/26))

# 1.0.0 (2019-03-30)

### Features
Enjoy! 🎈
