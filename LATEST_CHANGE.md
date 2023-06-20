# 3.4.0 (2023-06-21)

## Features

* **Database split**: Configurable field for document discrimination ([#150](https://github.com/matteobortolazzo/couchdb-net/issues/150))
* **Find**: Added all options and responses ([#182](https://github.com/matteobortolazzo/couchdb-net/issues/182))
* **Change feed**: Adds support for database split ([#187](https://github.com/matteobortolazzo/couchdb-net/issues/187))
* **Replicas**: Adds `CreateTarget` option ([#189](https://github.com/matteobortolazzo/couchdb-net/issues/189))

## Bugs

* **Queries**: Fix when `In` is called inside `Any` ([#183](https://github.com/matteobortolazzo/couchdb-net/issues/183))
* **Database split**: Fix `FirstOrDefault` without filter queries ([#185](https://github.com/matteobortolazzo/couchdb-net/issues/185))
