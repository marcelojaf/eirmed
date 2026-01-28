-- Create user if not exists
DO $$
BEGIN
  IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'eirmed') THEN
    CREATE ROLE eirmed WITH LOGIN PASSWORD 'eirmed_password';
  END IF;
END
$$;

-- Create database if not exists
SELECT 'CREATE DATABASE eirmed OWNER eirmed'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'eirmed')\gexec

-- Grant all privileges
GRANT ALL PRIVILEGES ON DATABASE eirmed TO eirmed;
