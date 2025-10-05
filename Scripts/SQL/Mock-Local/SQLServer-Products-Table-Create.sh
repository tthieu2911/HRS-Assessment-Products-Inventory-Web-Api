#!/bin/bash
if ! docker info >/dev/null 2>&1; then
  echo "❌ Docker daemon not running. Please start Docker first."
  exit 1
fi

# Verify connection
docker run -it --rm mcr.microsoft.com/mssql-tools:latest \
   /opt/mssql-tools/bin/sqlcmd -S host.docker.internal,1433 \
   -U sa -P "YourStrong!Passw0rd" -Q "SELECT @@VERSION;"

# Create DB if missing
docker run -it --rm mcr.microsoft.com/mssql-tools:latest \
   /opt/mssql-tools/bin/sqlcmd -S host.docker.internal,1433 \
   -U sa -P "YourStrong!Passw0rd" \
   -Q "IF DB_ID(N'InventoryDB') IS NULL CREATE DATABASE [InventoryDB];"

# Create schema if missing
docker run -it --rm mcr.microsoft.com/mssql-tools:latest \
   /opt/mssql-tools/bin/sqlcmd -S host.docker.internal,1433 \
   -U sa -P "YourStrong!Passw0rd" \
   -Q "IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = N'TestSchema') EXEC('CREATE SCHEMA [TestSchema]');"

# Create table if missing
docker run -it --rm mcr.microsoft.com/mssql-tools:latest \
  /opt/mssql-tools/bin/sqlcmd -S host.docker.internal,1433 \
  -U sa -P "YourStrong!Passw0rd" -d InventoryDB \
  -Q "IF OBJECT_ID(N'TestSchema.Products', N'U') IS NULL BEGIN CREATE TABLE [TestSchema].[Products] (InstitutionCode NVARCHAR(50) NOT NULL, ProductId NVARCHAR(50) NOT NULL, ProductName NVARCHAR(200) NOT NULL, InStocked INT NULL, Created NVARCHAR(50) NULL, Updated NVARCHAR(50) NULL, CONSTRAINT PK_Products PRIMARY KEY (InstitutionCode, ProductId)); END"