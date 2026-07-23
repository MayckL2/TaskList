#!/bin/bash
/opt/mssql/bin/sqlservr &
sleep 30
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "${DB_PASSWORD}" -C -Q "CREATE DATABASE IF NOT EXISTS TaskListDb"
wait