setlocal

docker run -it --rm mcr.microsoft.com/mssql-tools:latest ^
   /opt/mssql-tools/bin/sqlcmd -S host.docker.internal,1433 -U sa -P "YourStrong!Passw0rd" -Q "SELECT @@VERSION;"

docker run -it --rm mcr.microsoft.com/mssql-tools:latest ^
   /opt/mssql-tools/bin/sqlcmd -S host.docker.internal,1433 ^
   -U sa -P "YourStrong!Passw0rd" ^
   -Q "IF DB_ID(N'InventoryDB') IS NULL CREATE DATABASE [InventoryDB];"

docker run -it --rm mcr.microsoft.com/mssql-tools:latest ^
   /opt/mssql-tools/bin/sqlcmd -S host.docker.internal,1433 ^
   -U sa -P "YourStrong!Passw0rd" ^
   -Q "IF DB_ID(N'TestSchema') IS NULL CREATE SCHEMA [TestSchema];"

docker run -it --rm mcr.microsoft.com/mssql-tools:latest ^
  /opt/mssql-tools/bin/sqlcmd -S host.docker.internal,1433 ^
  -U sa -P "YourStrong!Passw0rd" -d InventoryDB ^
  -Q "IF OBJECT_ID(N'TestSchema.Products', N'U') IS NULL BEGIN CREATE TABLE [TestSchema].[Products] (InstitutionCode NVARCHAR(50) NOT NULL, ProductId NVARCHAR(50) NOT NULL, ProductName NVARCHAR(200) NOT NULL, InStocked INT NULL, Created NVARCHAR(50) NULL, Updated NVARCHAR(50) NULL, CONSTRAINT PK_Products PRIMARY KEY (InstitutionCode, ProductId)); END"


endlocal
