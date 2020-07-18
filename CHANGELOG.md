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
