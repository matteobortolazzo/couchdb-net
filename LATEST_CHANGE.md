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