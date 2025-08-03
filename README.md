# Telegram.API project (.NET 8)

## Overview

This template provides a clean architecture for building robust .NET 8 Web APIs. It includes:
- Layered solution structure (Domain, Application, Infrastructure, WebAPI)
- Dependency injection setup
- Exception handling middleware
- Health checks for SQL Server
- Automated code formatting and quality checks using [Husky.Net](https://github.com/alirezanet/Husky.Net)

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Git](https://git-scm.com/)
- Bash (for Unix) or CMD (for Windows) for Husky.Net hooks

### Setup Instructions

1. **Clone the repository**
```bash
git clone <your-repo-url> cd <repo-folder>
```
2. **Restore NuGet packages**
```bash
dotnet restore
```
3. **Update the connection string**
   - Edit `Telegram.API.WebAPI/appsettings.json` and set your SQL Server connection string under `ConnectionStrings:ConStr`.

4. **Build the solution**
```bash
dotnet build
```
5. **Run the application**
```bash
dotnet run --project Telegram.API.WebAPI
```
6. **Access Swagger UI**
   - Navigate to `https://localhost:<port>/swagger` in your browser.

## Husky.Net Setup & Usage

Husky.Net automates code quality checks (formatting, build, etc.) on Git hooks.

### Install Husky.Net as a dotnet tool
```bash
dotnet husky install

chmod +x .husky/*
```
### Initialize Husky.Net hooks

### Add Husky.Net tasks

Tasks are defined in `.husky/task-runner.json`. Example tasks:
- `dotnet-format` (pre-commit): Formats staged C# files.
- `dotnet-clean` (pre-push): Cleans the solution.
- `dotnet-build-with-warning-check` (pre-push): Builds and treats warnings as errors.

### Configure Git hooks

Hooks are defined in `.husky/pre-commit`, `.husky/pre-push`, etc.

**Example: `.husky/pre-commit`**
```bash
#!/bin/sh . "$(dirname "$0")/_/husky.sh"
dotnet husky run --group pre-commit dotnet husky run --group pre-push
```

### How it works

- On `git commit`, Husky.Net runs all tasks in the `pre-commit` group.
- On `git push`, Husky.Net runs all tasks in the `pre-push` group.

### Troubleshooting

- If hooks do not run, ensure you commit/push from the terminal (not just from an IDE).
- Make sure hooks are executable:
```bash
chmod +x .husky/*
```


## Customization

- Add or modify Husky.Net tasks in `.husky/task-runner.json`.
- Add new hooks using:
```bash
dotnet husky add <hook-name> -c "dotnet husky run --group <group-name>"
```

## 📦 Database Setup and Schema

To get your database up-and-running, we include all DDL and SP scripts in the `/Database` folder. Clone the repo and follow these steps:

1. **Create the database** in your SQL Server instance (e.g. `A2A_iMessaging`).
2. **Run table-creation scripts** in order:

   - `/Database/01-create-tables.sql` — defines `ReadyTable`, `ArchiveTable`, `RecentMessages`, and `BotChatMapping`.
3. **Run index-creation scripts** (included at bottom of `01-create-tables.sql`).
4. **Run stored-procedure scripts**:

   - `/Database/02-usps-enqueue-get-archive.sql` — contains:

     * `usp_EnqueueOrArchiveIfDuplicate` (inserts into `ReadyTable`, returns new ID)
     * `usp_GetUserByUsername`
     * `usp_GetChatId` (with OUTPUT parameter)
5. **Run trigger script**:

   - `/Database/03-trigger-archive-duplicates.sql` — defines `trg_ReadyTable_ArchiveDuplicates` to keep the 5-minute dedupe window.
6. **Schedule cleanup job**:

   - `/Database/04-job-purge-recentmessages.sql` — creates the SQL Agent job `Purge RecentMessages` to delete aged entries every minute.
---

### Folder Structure

```text
/  
├─ Database/
│  ├─ 01-create-tables.sql
│  ├─ 02-usps-enqueue-get-archive.sql
│  ├─ 03-trigger-archive-duplicates.sql
│  └─ 04-job-purge-recentmessages.sql
├─ src/        ← your application code
└─ README.md   ← this file
```

Once each script has executed successfully, your database schema and procedural logic will be in place.  Happy coding!


## License
MIT
