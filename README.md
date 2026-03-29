# Item Processing System 📦

> A full-stack ASP.NET Core MVC application managing items and their complex hierarchical parent-child relationships. 

This project was built to explore and solve real-world problems involving recursive data structures, relational mapping, and circular dependency prevention.

---

## 🚀 Key Features

* **CRUD Operations**: Complete management of items (Create, Read, Update, Delete)
* **Hierarchy Builder**: Link items together as parent and child nodes
* **Cycle Prevention**: Built-in algorithm prevents infinite loops and circular dependencies (e.g., A → B → C → A)
* **Tree Visualization**: Renders complex database relations into an easy-to-read UI hierarchy
* **Validation**: Robust server-side and client-side form validation
* **Transactional Deletions**: Prevents orphaned records in the database when deleting parent nodes

## 🛠️ Tech Stack

* **Backend:** C# / ASP.NET Core MVC (.NET 10.0)
* **Database:** Microsoft SQL Server
* **Data Access:** Custom ADO.NET Data Access Layer (`DbHelper`)
* **Frontend:** HTML5, Razor Views, Bootstrap 5, jQuery Unobtrusive Validation

## 💡 How It Works

1. **Items** have a Name and a Weight.
2. Items can be assigned as **Parents** to one or multiple **Children**.
3. When linking items, the system runs a deep **Cycle Check (DFS)**. If making `Item_A` a child of `Item_B` would cause a recursive loop, the action is blocked.
4. The **Tree View** maps out the current database state, finding "Root" items and recursively rendering their children.

## ⚙️ Getting Started

To run this project on your local machine, check out the comprehensive setup guide:

👉 **[Read the SETUP.md Guide Here](SETUP.md)**

The setup guide covers:
1. Spinning up a local SQL Server using Docker
2. Running the idempotent `setup.sql` script to create the tables
3. Connecting the application and compiling it via the .NET CLI

## 📂 Project Architecture

```text
ItemProcessingSystemCore/
├── Controllers/
│   ├── ItemController.cs      # Handles all CRUD & tree logic workflows
│   └── HomeController.cs      # Base routing
├── DAL/
│   └── DbHelper.cs            # Custom Data Access Layer executing SQL
├── Models/
│   ├── Item.cs                # Represents a single unit
│   └── ItemRelation.cs        # Mapping table logic for hierarchies
├── Views/
│   ├── Item/                  # Razor templates for Create, Edit, Process, Tree
│   └── Shared/                # Bootstrap layouts
├── wwwroot/                   # Static assets (css, js, libraries)
├── appsettings.json           # Connection strings
└── setup.sql                  # Database schema and indexes
```

## 📝 License & Contact

Developed by **Nikita Sachan**.  
This project was built for learning and demonstration purposes. Feel free to use and explore the code!
