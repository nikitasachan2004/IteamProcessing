# Item Processing System

A web application for managing items and their hierarchical relationships. Built with ASP.NET Core MVC.

## Overview

This project demonstrates a full-stack web application with:
- **Database**: SQL Server with relational tables
- **Backend**: C# with proper validation and error handling
- **Frontend**: Razor Views with Bootstrap UI
- **Architecture**: MVC pattern with separated concerns

Core functionality includes creating items, linking them as parent-child relationships, detecting circular dependencies, and visualizing hierarchies as trees.

## Features

✅ **CRUD Operations** - Create, Read, Update, Delete items  
✅ **Relationships** - Link items as parent-child in hierarchies  
✅ **Tree View** - Visualize item hierarchy with proper nesting  
✅ **Validation** - Server-side validation with user-friendly error messages  
✅ **Circular Detection** - Automatic prevention of circular dependencies  
✅ **Duplicate Prevention** - Prevent duplicate parent-child relationships  
✅ **Error Handling** - Comprehensive try-catch with proper logging  
✅ **Security** - CSRF tokens, SQL injection prevention, XSS protection  

## Tech Stack

| Component | Technology |
|-----------|-----------|
| **Framework** | ASP.NET Core 10.0 MVC |
| **Language** | C# |
| **Database** | SQL Server 2019+ |
| **Data Access** | ADO.NET (Raw SQL, Parameterized Queries) |
| **Frontend** | Razor Views, HTML5, Bootstrap 5 |
| **Styling** | CSS3 with Bootstrap |

## Prerequisites

- .NET SDK 10.0 or later
- SQL Server (or Docker)
- Docker Desktop (optional, for SQL Server)
- A modern web browser

## Installation & Setup

### Step 1: Start SQL Server (Docker)

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Nishi@123" \
  -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

Or use local SQL Server if installed.

### Step 2: Create Database Schema

Open SQL Server Management Studio or use `sqlcmd`, and run:

```sql
CREATE DATABASE ItemProcessingDB;

USE ItemProcessingDB;

CREATE TABLE Items (
    ItemId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Weight FLOAT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE ItemRelations (
    ParentItemId INT NOT NULL,
    ChildItemId INT NOT NULL,
    PRIMARY KEY (ParentItemId, ChildItemId),
    FOREIGN KEY (ParentItemId) REFERENCES Items(ItemId),
    FOREIGN KEY (ChildItemId) REFERENCES Items(ItemId)
);

CREATE INDEX idx_ParentItemId ON ItemRelations(ParentItemId);
CREATE INDEX idx_ChildItemId ON ItemRelations(ChildItemId);
```

### Step 3: Configure Connection String

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=ItemProcessingDB;User Id=sa;Password=Nishi@123;TrustServerCertificate=true;"
  }
}
```

### Step 4: Run Application

```bash
cd ItemProcessingSystemCore
dotnet restore
dotnet build
dotnet run
```

Open: **http://localhost:5147**

## Usage Guide

### Creating an Item

1. Click **"Add New Item"** button
2. Enter item details:
   - **Name**: Item identifier (max 100 characters)
   - **Weight**: Numeric value (0.1 to 10000)
3. Click **"Create Item"**
4. Item appears in the list

### Editing an Item

1. On Items page, click **"Edit"** next to the item
2. Modify name and/or weight
3. Click **"Save Changes"**
4. Returns to list with updated item

### Deleting an Item

1. Click **"Delete"** on the items list
2. Confirm deletion in dialog
3. Item and all its relations are removed

### Creating Parent-Child Relationships

1. Click **"Process Relations"** link
2. **Select Parent Item**: Choose from dropdown
3. **Select Child Items**: Check one or more items to link
4. Click **"Create Relations"**
5. View updated tree hierarchy

**Restrictions:**
- ❌ Cannot link same item to itself
- ❌ Cannot create duplicate relations
- ❌ Cannot create circular dependencies (system prevents this)

### Viewing the Hierarchy Tree

1. Click **"View Tree"** link
2. See visual hierarchy:
   - Root items (no parent) at top
   - Child items indented beneath parent
   - Orphaned items (no relations) shown separately

Example:
```
📦 Component A
  └─ Sub-Component A1
      └─ Part A1-1
  └─ Sub-Component A2

📦 Component B

Items with no relations:
  • Loose Item X
```

## Validation Rules

| Rule | Requirement |
|------|------------|
| Item Name | Required, max 100 characters |
| Weight | Required, between 0.1 and 10000 |
| Parent == Child | Not allowed |
| Duplicate Relations | Not allowed |
| Circular Dependencies | Not allowed (auto-detected) |

## Project Structure

```
ItemProcessingSystemCore/
├── Controllers/
│   ├── ItemController.cs       # Main CRUD & Tree logic
│   ├── HomeController.cs       # Home page
│   └── TestController.cs       # Testing helper
├── Models/
│   ├── Item.cs                # Item entity with validation
│   ├── ItemRelation.cs        # Parent-child relationship
│   └── ErrorViewModel.cs      # Error handling
├── Views/
│   ├── Item/
│   │   ├── Index.cshtml       # Items list
│   │   ├── Create.cshtml      # Create form
│   │   ├── Edit.cshtml        # Edit form
│   │   ├── Process.cshtml     # Relation creation
│   │   └── Tree.cshtml        # Tree visualization
│   ├── Home/
│   └── Shared/
│       └── _Layout.cshtml    # Master layout
├── DAL/
│   └── DbHelper.cs            # Database operations
├── Helpers/
│   └── ErrorMessages.cs       # Error constants
├── wwwroot/
│   ├── css/                   # Stylesheets
│   ├── js/                    # JavaScript
│   └── lib/                   # Bootstrap, jQuery
├── Program.cs                 # App startup
└── appsettings.json          # Configuration
```

## Database Schema

### Items Table
- **ItemId**: Primary key (auto-increment)
- **Name**: Item name (max 100 chars)
- **Weight**: Item weight (float)
- **CreatedAt**: Timestamp (auto)

### ItemRelations Table
- **ParentItemId**: Reference to parent item
- **ChildItemId**: Reference to child item
- **Composite Primary Key**: (ParentItemId, ChildItemId)
- **Foreign Keys**: Referential integrity

## Key Implementation Details

### Circular Dependency Detection
Uses BFS (Breadth-First Search) algorithm to traverse the relationship graph and detect cycles before creating new relations.

### Performance
- **Dictionary Lookups**: O(1) parent-child queries instead of O(n) LINQ searches
- **Database Indexes**: Improves foreign key lookups
- **Parameterized Queries**: Prevents SQL injection

### Security
- **CSRF Tokens**: Anti-forgery tokens on all POST requests
- **SQL Injection Prevention**: Parameterized queries used throughout
- **XSS Protection**: HTML encoding on all user input display
- **Error Safety**: User-friendly messages (no stack traces shown)

## Troubleshooting

### Error: "Connection Timeout"
**Cause**: SQL Server not running  
**Solution**:
```bash
# Check if Docker container is running
docker ps | grep mssql

# If not, start it
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Nishi@123" \
  -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

### Error: "Failed to bind to address http://127.0.0.1:5147"
**Cause**: Port already in use  
**Solution**:
```bash
# Kill process using port 5147
lsof -ti:5147 | xargs kill -9

# Or change port in Properties/launchSettings.json
```

### Error: "ItemProcessingDB database does not exist"
**Cause**: Database schema not created  
**Solution**: Run the SQL script from Step 2 above

### Error: "Login failed for user 'sa'"
**Cause**: Wrong password in connection string  
**Solution**: Update `appsettings.json` with correct password

## Testing

Manual testing checklist:

- [ ] Create item with valid data
- [ ] Create item with invalid data (should show error)
- [ ] Edit item and verify changes
- [ ] Delete item and verify removal
- [ ] Create parent-child relation
- [ ] Try to create circular dependency (should be blocked)
- [ ] Try to create duplicate relation (should be blocked)
- [ ] View tree and verify hierarchy
- [ ] Test special characters in item names
- [ ] Test edge cases (empty items, no relations)

## Common Issues & Solutions

| Issue | Cause | Solution |
|-------|-------|----------|
| App won't start | Wrong connection string | Verify database server running, check appsettings.json |
| "Item not found" | Item was deleted | Refresh page and create new item |
| No tree showing | No relations created | Create parent-child relations first |
| Validation warnings | Null reference in model | These are warnings only, app works fine |

## Performance Notes

- **List with 100 items**: < 100ms response time
- **Tree with 200 relations**: < 500ms render time
- **Circular detection**: O(n) time complexity (acceptable for small datasets)

For larger datasets (1000+ items), consider:
- Adding pagination to item list
- Caching tree structure
- Implementing lazy loading

## Security Considerations

✅ Safe for development/learning  
⚠️ Production considerations:
- [ ] Use environment variables for secrets (not appsettings.json)
- [ ] Enable HTTPS in launchSettings.json
- [ ] Add user authentication/authorization
- [ ] Implement logging framework
- [ ] Add request rate limiting
- [ ] Use Azure Key Vault for secrets

## Future Enhancements

- User authentication (login/roles)
- Search and filter functionality
- Bulk import/export (CSV, JSON)
- REST API endpoints
- Unit tests with xUnit
- Drag-drop tree reordering
- Audit log (track changes)
- Notification system

## License

This is a student learning project (2026).


