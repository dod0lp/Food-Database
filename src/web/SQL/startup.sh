#!/bin/bash

/opt/mssql/bin/sqlservr &

echo "[-----------] Waiting for SQL Server to start... [-----------]"

until /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P ${SA_PASSWORD} -Q "SELECT 1" &>/dev/null
do
  echo "[-----------] SQL Server is not yet available. Waiting... [-----------]"
  sleep 5s
done

echo "[-----------] Running initialization script and starting up database. [-----------]"
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P ${SA_PASSWORD} -i /usr/src/app/init.sql -v DB_NAME=${DB_NAME} DB_USER="${DB_USER}" DB_PASSWORD="${DB_PASSWORD}"

wait
