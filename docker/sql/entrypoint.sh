#!/bin/bash
/opt/mssql/bin/sqlservr &
SQL_PID=$!
sleep 45

# Retry connection with timeout
for i in {1..10}; do
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "${DB_PASSWORD}" -C -Q "CREATE DATABASE IF NOT EXISTS TaskListDb" 2>/dev/null && break
  echo "Attempt $i: Retrying connection..."
  sleep 3
done

wait $SQL_PID
