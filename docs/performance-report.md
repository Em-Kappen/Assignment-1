# Performance report status

The measurements previously recorded in this file were collected against the
earlier demo database and do not describe the existing SQL_main database. They
are not comparable to the current Customer, Order, and Product OData
integration.

Benchmark execution and benchmark-history persistence are disabled for
SQL_main. Use read-only, scoped OData requests from
[`samples/odata-requests.http`](../samples/odata-requests.http) when evaluating
response behavior. Database execution plans and performance testing should be
planned separately against the SQL_main environment.
