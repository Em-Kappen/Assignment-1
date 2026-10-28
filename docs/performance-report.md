# Performance Benchmark Report

## Executive summary

The benchmark suite completed all 50 requests successfully: ten OData scenarios
were executed five times each, with **50 successes and zero failures**. Every
recorded HTTP status was `200`.

The dataset contained **3,650,000 business rows**:

| Table | Rows |
| --- | ---: |
| Customers | 100,000 |
| Products | 50,000 |
| Orders | 500,000 |
| Order items | 2,000,000 |
| Employees | 1,000,000 |
| **Total** | **3,650,000** |

The UI summary reported an average scenario time of **7,818.2 ms**. The fastest
observed individual run was **4.5 ms** for the top-paid employee query. The
slowest was **32,248.4 ms** for orders expanded with both customers and order
items.

## Run details

- **Reported run date:** October 27, 2026
- **Workload:** Ten configured scenarios, five sequential requests per scenario
- **Total requests:** 50
- **Successes / failures:** 50 / 0
- **HTTP statuses:** `200` for all recorded requests
- **Dataset size:** 3,650,000 business rows
- **Payload size:** The UI's reported payload size for each scenario

## Scenario results

| Scenario | Min (ms) | Max (ms) | Average (ms) | Median (ms) | Payload (bytes) | Success / failure |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Customers top 100 | 9.6 | 66.8 | 21.9 | 10.3 | 29,006 | 5 / 0 |
| Customers top 1000 | 61.6 | 77.0 | 67.8 | 64.8 | 292,896 | 5 / 0 |
| Electronics products | 138.0 | 394.1 | 208.4 | 175.2 | 2,737,263 | 5 / 0 |
| IT employees | 1,319.8 | 1,785.3 | 1,479.3 | 1,413.9 | 33,289,894 | 5 / 0 |
| Top-paid employees | 4.5 | 19.0 | 8.1 | 5.2 | 23,961 | 5 / 0 |
| Orders with customer | 10,711.9 | 11,518.6 | 11,041.9 | 10,750.8 | 263,205,639 | 5 / 0 |
| Orders with items | 22,130.7 | 23,604.6 | 22,987.2 | 23,134.4 | 337,087,627 | 5 / 0 |
| Orders with customer and items | 30,260.7 | 32,248.4 | 31,037.4 | 31,056.8 | 491,478,418 | 5 / 0 |
| Orders with count | 4,177.7 | 4,553.4 | 4,360.2 | 4,367.5 | 108,814,869 | 5 / 0 |
| Customers with orders | 6,696.3 | 7,415.8 | 6,969.5 | 6,863.5 | 139,693,016 | 5 / 0 |

The overall average is the arithmetic mean of the ten displayed scenario
averages (about 7,818.2 ms). It is not a request-weighted or percentile
measurement.

## Interpretation

### Limited queries performed substantially better

The two customer queries and the top-paid employee query returned limited
result sets and had average latencies from **8.1 ms to 67.8 ms**. The employee
sorting scenario had a reported minimum of 4.5 ms, though its maximum was
19.0 ms; the gap is a reminder that five samples are a small set for drawing
conclusions about variability.

The Electronics product filter averaged **208.4 ms** and returned about
**2.7 MB**. The IT employee filter averaged **1,479.3 ms** with a payload of
about **33.3 MB**. These filtered requests do not specify `$top`, so their
latency and transfer size reflect the full matching result set rather than a
small page.

### Relationship expansions were payload- and transfer-intensive

The expansion scenarios returned very large response bodies:

- Orders with customer: about **263.2 MB**, average **11.0 s**.
- Orders with items: about **337.1 MB**, average **23.0 s**.
- Orders with both relations: about **491.5 MB**, average **31.0 s**.
- Customers with orders: about **139.7 MB**, average **7.0 s**.

The combined order expansion was the slowest workload, consistent with its
largest payload. These are end-to-end HTTP timings, so database execution,
OData materialization/serialization, and local transfer all contribute. The
results do not isolate PostgreSQL query execution time.

### The count scenario is not an isolated count measurement

The configured benchmark request is `/odata/Orders?$count=true` with no
`$top`. It returns the order collection as well as the count, which explains
the approximately **108.8 MB** payload and multi-second response. It should not
be interpreted as a count-only benchmark. To assess count latency separately,
use a request such as `/odata/Orders?$count=true&$top=0`.

## Indexes observed

The database index summary showed indexes relevant to the tested lookups,
including:

- `ix_products_category`
- `ix_employees_department`
- `ix_employees_salary_desc`
- `ix_orders_customer_id`
- `ix_orders_order_date`
- `ix_orders_order_status`
- `ix_order_items_order_id`
- `ix_order_items_product_id`

The presence of an index does not establish that PostgreSQL used it for a
particular request. Use `EXPLAIN (ANALYZE, BUFFERS)` on representative
queries to inspect plans, rows scanned, and buffer activity.

## Recommendations

1. Add `$top` to broad filtered and expanded UI queries for interactive use.
   Keep unbounded expansion requests as explicit stress scenarios.
2. Benchmark counts separately with `$count=true&$top=0` so result transfer
   does not dominate the measurement.
3. Use server-driven paging or client paging for large result sets and review
   response-size limits and timeouts for production deployments.
4. Repeat measurements under a documented, controlled environment. Record
   hardware, PostgreSQL version/settings, application build, warm/cold cache
   conditions, and concurrent load before comparing runs.
5. Inspect query plans with `EXPLAIN (ANALYZE, BUFFERS)` before making index
   changes. Compare medians and distributions across repeated runs rather than
   relying only on a single minimum or maximum.

## Methodology and caveats

The benchmark runner makes five sequential HTTP requests for each of the ten
configured scenarios and stores per-scenario minimum, maximum, average, median,
success/failure counts, status codes, dataset size, and a payload byte count.
The measurements here are transcribed from the UI results shared for this run.

The UI table displays the payload size recorded for a scenario, not the sum of
all five responses. The benchmark implementation reads every response body to
measure bytes. Broad expansion requests therefore exercise both query execution
and substantial serialization and transfer work. Results are specific to this
run and its environment; they should not be treated as database-only timings or
as a guarantee for other hardware, network conditions, or workloads.
