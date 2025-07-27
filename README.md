# API Template (.NET 8)

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
   - Edit `API.Template.WebAPI/appsettings.json` and set your SQL Server connection string under `ConnectionStrings:DbConnectionString`.

4. **Build the solution**
```bash
dotnet build
```
5. **Run the application**
```bash
dotnet run --project API.Template.WebAPI
```
6. **Access Swagger UI**
   - Navigate to `https://localhost:<port>/swagger` in your browser.

## Husky.Net Setup & Usage

Husky.Net automates code quality checks (formatting, build, etc.) on Git hooks.

### Install Husky.Net as a dotnet tool
```bash
dotnet husky install
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


## License
MIT
