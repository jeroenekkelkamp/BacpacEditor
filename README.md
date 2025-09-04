# BacpacEditor

A .NET 8 command-line tool for editing SQL Server BACPAC files by removing specific database objects like stored procedures, views, functions, and more.

## Why Use BacpacEditor?

When exporting a database as a BACPAC file, SQL Server includes **everything**.. tables, stored procedures, views, functions, triggers, constraints, and more. Sometimes you only want to import specific parts of the database, but BACPAC files don't allow selective imports.

**Common scenarios where BacpacEditor helps:**

- **Data migration**: You want to import only tables and data, not stored procedures or views
- **Development environments**: Remove production-specific stored procedures before importing to dev
- **Clean database setup**: Import only the schema without business logic (stored procedures/functions)
- **Compliance**: Remove sensitive stored procedures before sharing database exports
- **Testing**: Create test databases without certain objects that might interfere with testing

## Setup

### Prerequisites
- .NET 8.0 SDK or later

### Build from Source
```bash
git clone <repository-url>
cd BacpacEditor
dotnet build
```

## Usage

### Basic Syntax
```bash
dotnet run -- <file-path> <selection-options>
```

### Selection Options

| Option | Description |
|--------|-------------|
| `--Views` | Remove all views |
| `--StoredProcedures` | Remove all stored procedures |
| `--Functions` | Remove all functions (scalar, table-valued, etc.) |
| `--Tables` | Remove all tables |
| `--Indexes` | Remove all indexes |
| `--Constraints` | Remove all constraints |
| `--Triggers` | Remove all triggers |
| `--Schemas` | Remove all schemas |
| `--Users` | Remove all users |
| `--Roles` | Remove all roles |
| `--AllObjects` | Remove all database objects |

### Examples

#### Remove Views and Stored Procedures
```bash
dotnet run -- "database.bacpac" "--Views" "--StoredProcedures"
```

#### Remove Functions and Triggers
```bash
dotnet run -- "database.bacpac" "--Functions" "--Triggers"
```

#### Remove Everything Except Tables
```bash
dotnet run -- "database.bacpac" "--Views" "--StoredProcedures" "--Functions" "--Triggers" "--Indexes" "--Constraints"
```

#### Work with XML Files Directly
```bash
dotnet run -- "model.xml" "--Views"
```

## How It Works

### BACPAC Files
1. **Extracts** the BACPAC file (which is a ZIP archive)
2. **Modifies** the `model.xml` file to remove specified elements
3. **Updates** the `origin.xml` file with new checksums and object counts
4. **Repackages** everything into a new `_modified.bacpac` file
5. **Cleans up** temporary files

### XML Files
1. **Loads** the XML file
2. **Removes** specified elements based on Type attributes
3. **Saves** the modified XML file
