You need a CouchDB database engine running on http://localhost:5984/.

If you have a docker execute:
```
docker run -p 5984:5984 -e COUCHDB_USER=admin -e COUCHDB_PASSWORD=admin -d couchdb
```