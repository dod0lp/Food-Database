#!/bin/bash

/opt/mssql/bin/sqlservr &

echo "[-----------] Waiting for SQL Server to start... [-----------]"


until /opt/mssql-tools18/bin/sqlcmd -S "${DB_SERVER}" \
  -U sa -P ${SA_PASSWORD} \
  -Q "SELECT 1" &>/dev/null \
  -C
do
  echo "[-----------] SQL Server is not yet available. Waiting... [-----------]"
  sleep 5s
done

echo "[-----------] Running initialization script and starting up database. [-----------]"
/opt/mssql-tools18/bin/sqlcmd -S "${DB_SERVER}" \
  -U sa -P ${SA_PASSWORD} \
  -i ${WORK_DIR}/init.sql \
  -v WORK_DIR="${WORK_DIR}" DB_NAME="${DB_NAME}" \
      DB_USER="${DB_USER}" DB_PASSWORD="${DB_PASSWORD}" \
  -C

wait