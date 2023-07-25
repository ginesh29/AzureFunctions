
# Azure Function

## Installation

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
Clone Project from https://github.com/ginesh29/AzureFunctions.git
```
