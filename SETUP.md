# How to Run — Item Processing System

Complete step-by-step guide to set up SQL Server, configure the database, and run the application locally.

---

## Requirements

| Tool | Version | Notes |
|------|---------|-------|
| .NET SDK | 9.0 or later | `dotnet --version` to check |
| SQL Server | 2019 / 2022 | Local install **or** Docker |
| Docker Desktop | Any recent | Only needed if using Docker for SQL Server |
| Web Browser | Chrome / Edge / Firefox | To access the app |

---

## Option A — SQL Server via Docker (Recommended)

If you do not have SQL Server installed locally, use Docker. This is the easiest option.

**Step 1 — Pull and start SQL Server:**

```bash
docker run \
  -e "ACCEPT_EULA=Y" \
  -e "SA_PASSWORD=Nishi@123" \
  -p 1433:1433 \
  --name sql-itemprocessing \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

**Step 2 — Wait ~15 seconds, then verify it is running:**

```bash
docker ps
```

You should see `sql-itemprocessing` in the list with status `Up`.

---

## Option B — Local SQL Server (Already Installed)

If SQL Server is already installed on your machine:

1. Open **SQL Server Management Studio (SSMS)** or **Azure Data Studio**
2. Connect to `localhost` using `sa` / `Nishi@123`
3. Confirm the server is running in **SQL Server Configuration Manager → SQL Server Services**

---

## Step 2 — Create the Database and Tables

Run the provided `setup.sql` script. You only need to do this **once**.

### Using SSMS / Azure Data Studio

1. Open SSMS and connect to your server
2. Click **File → Open → File** and select `setup.sql` (in the project root)
3. Click **Execute** (or press `F5`)
4. At the bottom you will see a results grid showing the two tables: `Items` and `ItemRelations`

### Using sqlcmd (Command Line)

```bash
sqlcmd -S localhost,1433 -U sa -P "Nishi@123" -i setup.sql
```

### What the script creates

```
Database: ItemProcessingDB
│
├── Table: Items
│   ├── ItemId    INT (PK, auto-increment)
│   ├── Name      NVARCHAR(100) NOT NULL
│   ├── Weight    FLOAT NOT NULL
│   └── CreatedAt DATETIME (auto, default GETDATE())
│
└── Table: ItemRelations
    ├── RelationId   INT (PK, auto-increment)
    ├── ParentItemId INT (FK → Items.ItemId)
    ├── ChildItemId  INT (FK → Items.ItemId)
    └── UNIQUE constraint on (ParentItemId, ChildItemId)
```

> **Note:** The script is idempotent — you can run it multiple times safely.
> It uses `IF NOT EXISTS` guards so it will not fail or duplicate anything.

---

## Step 3 — Configure the Connection String

Open `appsettings.json` in the project root. It should look like this:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=ItemProcessingDB;User Id=sa;Password=Nishi@123;TrustServerCertificate=True;"
  }
}
```

| Setting | Value |
|---------|-------|
| `Server` | `localhost,1433` |
| `Database` | `ItemProcessingDB` |
| `User Id` | `sa` |
| `Password` | `Nishi@123` |
| `TrustServerCertificate` | `True` (required for local dev) |

> If your SQL Server is on a different host or port, update `Server` accordingly.

---

## Step 4 — Run the Application

Open a terminal in the project folder and run:

```bash
cd ItemProcessingSystemCore

dotnet restore

dotnet run
```

Expected output:

```
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5147
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

Open your browser and go to:

```
http://localhost:5147/Item
```

---

## Step 5 — Using the Application

### Creating Items
1. Click **Items** in the navbar → **Add New Item**
2. Enter a Name (max 100 characters) and Weight (0.1 – 10000)
3. Click **Create Item**

### Processing (Parent → Child Relations)
1. Click **Process** in the navbar
2. Select a **Parent Item** from the dropdown
3. Check one or more **Child Items**
4. Click **Create Relations**
5. The app will block:
   - Same item as both parent and child
   - Duplicate relations
   - Circular dependencies

### Viewing the Tree
1. Click **Tree** in the navbar
2. The full hierarchy is shown with nesting
3. Unlinked items (no relations) are shown separately at the bottom

---

## Troubleshooting

### "A connection was successfully established with the server, but then an error occurred"
**Cause:** Wrong password or `TrustServerCertificate` missing  
**Fix:** Verify `appsettings.json` matches the password you set when starting Docker

---

### "Cannot open database 'ItemProcessingDB'"
**Cause:** The database does not exist yet  
**Fix:** Run `setup.sql` as described in Step 2

---

### "Login failed for user 'sa'"
**Cause:** Wrong password in connection string  
**Fix:** Update `appsettings.json` — password must match exactly what you used in the Docker run command

---

### "Failed to bind to address http://127.0.0.1:5147"
**Cause:** Port 5147 is already in use  
**Fix:**
```bash
# Find and kill the process on macOS/Linux:
lsof -ti:5147 | xargs kill -9

# Or change the port in Properties/launchSettings.json
```

---

### Docker container stopped after restart
**Fix:** Start it again:
```bash
docker start sql-itemprocessing
```

---

## Full SQL Script Reference

Below is the full contents of `setup.sql` for reference:

```sql
-- Create database (skipped if already exists)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'ItemProcessingDB')
BEGIN
    CREATE DATABASE ItemProcessingDB;
END
GO

USE ItemProcessingDB;
GO

-- Items table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Items')
BEGIN
    CREATE TABLE Items (
        ItemId    INT           NOT NULL PRIMARY KEY IDENTITY(1,1),
        Name      NVARCHAR(100) NOT NULL,
        Weight    FLOAT         NOT NULL,
        CreatedAt DATETIME      NOT NULL DEFAULT GETDATE()
    );
END
GO

-- ItemRelations table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ItemRelations')
BEGIN
    CREATE TABLE ItemRelations (
        RelationId   INT NOT NULL PRIMARY KEY IDENTITY(1,1),
        ParentItemId INT NOT NULL,
        ChildItemId  INT NOT NULL,

        CONSTRAINT UQ_ItemRelations UNIQUE (ParentItemId, ChildItemId),

        CONSTRAINT FK_ItemRelations_Parent FOREIGN KEY (ParentItemId)
            REFERENCES Items(ItemId),
        CONSTRAINT FK_ItemRelations_Child FOREIGN KEY (ChildItemId)
            REFERENCES Items(ItemId)
    );
END
GO

-- Performance indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ItemRelations_Parent')
    CREATE INDEX IX_ItemRelations_Parent ON ItemRelations(ParentItemId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ItemRelations_Child')
    CREATE INDEX IX_ItemRelations_Child ON ItemRelations(ChildItemId);
GO
```

---

*Last updated: March 2026*
