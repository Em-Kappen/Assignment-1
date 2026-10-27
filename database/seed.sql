DO $seed$
BEGIN
    PERFORM pg_advisory_xact_lock(774839201);

    IF to_regclass('public.database_metadata') IS NULL OR to_regclass('public.customers') IS NULL THEN
        RAISE EXCEPTION 'Run database/schema.sql before database/seed.sql';
    END IF;

    IF EXISTS (SELECT 1 FROM public.database_metadata WHERE key = 'BenchmarkSeedVersion') THEN
        RAISE NOTICE 'Benchmark dataset already initialized; no rows inserted';
        RETURN;
    END IF;

    IF EXISTS (SELECT 1 FROM public.customers LIMIT 1)
       OR EXISTS (SELECT 1 FROM public.products LIMIT 1)
       OR EXISTS (SELECT 1 FROM public.orders LIMIT 1)
       OR EXISTS (SELECT 1 FROM public.order_items LIMIT 1)
       OR EXISTS (SELECT 1 FROM public.employees LIMIT 1) THEN
        RAISE EXCEPTION 'Benchmark tables contain data but no seed marker exists; refusing to duplicate or mix datasets';
    END IF;

    INSERT INTO public.branches (branch_id, name, city)
    SELECT n, 'Branch ' || lpad(n::text, 3, '0'), 'City ' || ((n - 1) % 40 + 1)
    FROM generate_series(1, 100) AS sequence(n);

    INSERT INTO public.customers
        (customer_id, customer_number, first_name, last_name, email, phone,
         address_line1, city, state, country, created_date, is_active)
    SELECT n,
           'CUST-' || lpad(n::text, 8, '0'),
           (ARRAY['Avery','Jordan','Taylor','Morgan','Casey','Riley','Jamie','Alex'])[((n - 1) % 8) + 1],
           (ARRAY['Smith','Patel','Garcia','Kim','Brown','Singh','Miller','Chen'])[((n * 7 - 1) % 8) + 1],
           'customer' || n || '@example.test',
           '+1-555-' || lpad((n % 10000000)::text, 7, '0'),
           (n % 9000 + 1) || ' Market Street',
           'City ' || ((n - 1) % 250 + 1),
           (ARRAY['CA','NY','TX','WA','IL','FL','MA','CO'])[((n * 3 - 1) % 8) + 1],
           (ARRAY['USA','Canada','UK','Germany','Australia'])[((n * 11 - 1) % 5) + 1],
           TIMESTAMP '2018-01-01' + ((n * 17) % 3000) * INTERVAL '1 day',
           n % 23 <> 0
    FROM generate_series(1, 100000) AS sequence(n);

    INSERT INTO public.products
        (product_id, sku, product_name, category, description, unit_price,
         stock_quantity, created_date, is_active, branch_id)
    SELECT n,
           'SKU-' || lpad(n::text, 7, '0'),
           (ARRAY['Wireless Headphones','Smart Display','Office Chair','Storage Drive','Network Adapter',
                  'Coffee Maker','Desk Lamp','Tablet Stand'])[((n * 5 - 1) % 8) + 1] || ' ' || n,
           (ARRAY['Electronics','Office','Furniture','Accessories','Appliances'])[((n * 13 - 1) % 5) + 1],
           'Product specification and catalog item ' || n,
           (499 + (n * 73 % 250000))::numeric / 100,
           (n * 37) % 5000,
           TIMESTAMP '2020-01-01' + ((n * 19) % 2200) * INTERVAL '1 day',
           n % 31 <> 0,
           ((n - 1) % 100 + 1)
    FROM generate_series(1, 50000) AS sequence(n);

    INSERT INTO public.orders
        (order_id, customer_id, order_number, order_date, order_status,
         subtotal, tax_amount, shipping_amount, total_amount)
    SELECT n,
           (((n::bigint * 7919 - 1) % 100000) + 1)::integer,
           'ORD-' || lpad(n::text, 9, '0'),
           TIMESTAMP '2021-01-01' + ((n * 7) % 2100) * INTERVAL '1 day',
           (ARRAY['Pending','Processing','Shipped','Delivered','Cancelled'])[((n * 3 - 1) % 5) + 1],
           totals.subtotal,
           round(totals.subtotal * 0.0825, 2),
           CASE WHEN n % 7 = 0 THEN 0 ELSE 9.95 END,
           totals.subtotal + round(totals.subtotal * 0.0825, 2) +
               CASE WHEN n % 7 = 0 THEN 0 ELSE 9.95 END
    FROM generate_series(1, 500000) AS sequence(n)
    CROSS JOIN LATERAL (
        SELECT sum(q.quantity * p.unit_price)::numeric(14,2) AS subtotal
        FROM generate_series(1, 4) AS lines(line_no)
        CROSS JOIN LATERAL (
            SELECT ((n * 7 + line_no * 997 - 1) % 50000 + 1) AS product_id,
                   ((n + line_no * 3) % 5 + 1) AS quantity
        ) q
        JOIN public.products p ON p.product_id = q.product_id
    ) totals;

    INSERT INTO public.order_items
        (order_item_id, order_id, product_id, quantity, unit_price, line_total)
    SELECT (o.order_id - 1) * 4 + line_no,
           o.order_id,
           ((o.order_id * 7 + line_no * 997 - 1) % 50000 + 1),
           ((o.order_id + line_no * 3) % 5 + 1),
           p.unit_price,
           ((o.order_id + line_no * 3) % 5 + 1) * p.unit_price
    FROM public.orders o
    CROSS JOIN generate_series(1, 4) AS lines(line_no)
    JOIN public.products p ON p.product_id =
        ((o.order_id * 7 + line_no * 997 - 1) % 50000 + 1);

    INSERT INTO public.employees
        (employee_id, employee_number, first_name, last_name, department, designation,
         manager_id, salary, hire_date, branch, branch_id)
    SELECT n,
           'EMP-' || lpad(n::text, 8, '0'),
           (ARRAY['Avery','Jordan','Taylor','Morgan','Casey','Riley','Jamie','Alex'])[((n * 3 - 1) % 8) + 1],
           (ARRAY['Smith','Patel','Garcia','Kim','Brown','Singh','Miller','Chen'])[((n * 5 - 1) % 8) + 1],
           (ARRAY['IT','Sales','Finance','Operations','Support','Engineering','Marketing'])[((n * 17 - 1) % 7) + 1],
           (ARRAY['Analyst','Specialist','Coordinator','Manager','Director'])[((n * 11 - 1) % 5) + 1],
           CASE WHEN n <= 100 THEN NULL ELSE ((n - 1) / 10)::integer END,
           (4000000 + (n * 137 % 16000000))::numeric / 100,
           DATE '2000-01-01' + ((n * 13) % 9000)::integer,
           'Branch ' || lpad((((n - 1) % 100 + 1))::text, 3, '0'),
           ((n - 1) % 100 + 1)
    FROM generate_series(1, 1000000) AS sequence(n);

    INSERT INTO public.database_metadata (key, value)
    VALUES ('BenchmarkSeedVersion', '1');

    PERFORM setval(pg_get_serial_sequence('public.branches', 'branch_id'),
                   (SELECT max(branch_id) FROM public.branches), TRUE);
    PERFORM setval(pg_get_serial_sequence('public.customers', 'customer_id'),
                   (SELECT max(customer_id) FROM public.customers), TRUE);
    PERFORM setval(pg_get_serial_sequence('public.products', 'product_id'),
                   (SELECT max(product_id) FROM public.products), TRUE);
    PERFORM setval(pg_get_serial_sequence('public.orders', 'order_id'),
                   (SELECT max(order_id) FROM public.orders), TRUE);
    PERFORM setval(pg_get_serial_sequence('public.order_items', 'order_item_id'),
                   (SELECT max(order_item_id) FROM public.order_items), TRUE);
    PERFORM setval(pg_get_serial_sequence('public.employees', 'employee_id'),
                   (SELECT max(employee_id) FROM public.employees), TRUE);

    ANALYZE public.customers;
    ANALYZE public.products;
    ANALYZE public.orders;
    ANALYZE public.order_items;
    ANALYZE public.employees;
END;
$seed$;
