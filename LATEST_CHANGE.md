## Breaking Changes
* Update to [Flurl 3](https://github.com/tmenier/Flurl/releases/tag/Flurl.Http.3.0.0). There should be no differences for the end user, but keep in mind.

## Features
* **Table Splitting**: Ability to use the same database for different document with automatic filtering. ([#106](https://github.com/matteobortolazzo/couchdb-net/issues/106))
* **Views**: Ability to get views. Thanks to [panoukos41](https://github.com/panoukos41) ([#117](https://github.com/matteobortolazzo/couchdb-net/issues/117))

## Improvements
* **Logical Expressions Prune**: If expressions are constant booleans, they are removed automatically keeping the query valid. ([#113](https://github.com/matteobortolazzo/couchdb-net/issues/113))
* **IsUpAsync**: Returns false on timeout and on not successful codes. ([#107](https://github.com/matteobortolazzo/couchdb-net/issues/107))
* **FindAsync**: Faster when document is not found. ([#92](https://github.com/matteobortolazzo/couchdb-net/issues/92))