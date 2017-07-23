@echo off
set POSTGRES_BIN_DIR=C:\Program Files\PostgreSQL\9.5\bin\
set SCRIPT_DIR=%~dp0%
set POSTGRES_DATA=C:\Program Files\PostgreSQL\9.5\data
set PG_CTL=%POSTGRES_BIN_DIR%pg_ctl
set CMD="%PG_CTL%" -D "%POSTGRES_DATA%" start

%CMD%