# 4.1.0

## Bugs

* **Find queries**: `_id` and `_rev` fields from CouchDB `_find` responses are now correctly mapped to `Id` and `Rev` properties on result documents. Previously, these fields were silently dropped during deserialization. ([#219](https://github.com/matteobortolazzo/couchdb-net/pull/219), based on [#218](https://github.com/matteobortolazzo/couchdb-net/pull/218)) Thanks [@gchiappe](https://github.com/gchiappe)
* **Active tasks**: Fixed `GetActiveTasksAsync()` throwing `InvalidOperationException` at runtime due to `UnixTimestampSecondsConverter` type mismatch with `ActiveTask.StartedOn`/`UpdatedOn` properties ([#219](https://github.com/matteobortolazzo/couchdb-net/pull/219))
* **Changes feed**: Fixed `ReceiveStream()` prematurely disposing the HTTP response content stream when using POST-based continuous change feed filters ([#219](https://github.com/matteobortolazzo/couchdb-net/pull/219))

## Improvements

* Extracted shared `DocumentRewriter` for `_id`/`_rev` property rewriting, used by both `FindResultConverter` and `ReadItemResponseConverter`
