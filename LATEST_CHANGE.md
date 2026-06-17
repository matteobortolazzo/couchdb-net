# 4.1.1

## Bugs

* **Find queries**: `Select` projections now deserialize correctly. `_id`/`_rev` are mapped to `Id`/`Rev` in anonymous projections (e.g. `Select(x => new { x.Id })`), and scalar member projections (e.g. `Select(x => x.Id)`, `Select(x => x.Name)`, `Select(x => x.Age)`) now return the selected field's value instead of throwing during deserialization. ([#217](https://github.com/matteobortolazzo/couchdb-net/issues/217))
