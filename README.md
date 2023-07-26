
# Azure Function

To run project locally

1. Create Database
```bash
CREATE DATABASE databasename;
```
2. Create Table
```bash
CREATE TABLE [dbo].[Todos]
(
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [TaskDescription] NVARCHAR(50) NOT NULL, 
    [IsCompleted] BIT NOT NULL, 
    [CreatedTime] DATETIME NOT NULL
)
```
3. Clone Repository
```bash
git clone https://github.com/ginesh29/AzureFunctions.git
```
