INSERT INTO branches (id, name, city)
VALUES
    (1, 'Chennai Branch', 'Chennai'),
    (2, 'Bangalore Branch', 'Bangalore');

INSERT INTO employees (id, name, email, salary, branchid)
VALUES
    (1, 'Lynn Matthew', 'lynn@company.com', 70000.00, 1),
    (2, 'John Smith', 'john@company.com', 60000.00, 2),
    (3, 'Sarah Wilson', 'sarah@company.com', 85000.00, 2),
    (4, 'Clark Kent', 'clark@company.com', 70000.00, 1);

INSERT INTO products (id, name, price, branchid)
VALUES
    (1, 'Laptop', 70000.00, 1),
    (2, 'Keyboard', 2500.00, 2),
    (3, 'Mouse', 1200.00, 1);