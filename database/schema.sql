DROP TABLE IF EXISTS products;
DROP TABLE IF EXISTS employees;
DROP TABLE IF EXISTS branches;

CREATE TABLE branches (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    city VARCHAR(255) NOT NULL
);

CREATE TABLE employees (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    salary NUMERIC(18,2) NOT NULL,
    branchid INTEGER NOT NULL,
    CONSTRAINT fk_employees_branch
        FOREIGN KEY (branchid)
        REFERENCES branches(id)
        ON DELETE CASCADE
);

CREATE TABLE products (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    price NUMERIC(18,2) NOT NULL,
    branchid INTEGER NOT NULL,
    CONSTRAINT fk_products_branch
        FOREIGN KEY (branchid)
        REFERENCES branches(id)
        ON DELETE CASCADE
);