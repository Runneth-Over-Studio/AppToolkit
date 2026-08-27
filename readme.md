![App Toolkit Logo](https://raw.githubusercontent.com/Runneth-Over-Studio/AppToolkit/refs/heads/main/content/icon-175.png)

# App Toolkit
Runneth Over Studio's common C# app development code.

## Purpose
This library is designed to accelerate the development of prototypes, in-house utilities, and narrowly focused applications. It is not intended for use in large-scale, robust enterprise systems. The primary goal is to enable rapid domain-specific feature development by providing reusable components and eliminating the need to maintain boilerplate code across multiple projects. By leveraging this library, developers can focus on delivering core functionality without being burdened by repetitive infrastructure concerns.

## Opinionated Design and Hard Dependencies
App Toolkit is intentionally opinionated. It favors a practical, batteries-included baseline for common app concerns over strict minimalism. Even if your application uses only part of the API surface, the package currently has several hard dependencies by design.

Major dependencies include:

- [CommunityToolkit.Mvvm](https://www.nuget.org/packages/CommunityToolkit.Mvvm)
- [Dapper](https://www.nuget.org/packages/Dapper)
- [Microsoft.Data.Sqlite](https://www.nuget.org/packages/Microsoft.Data.Sqlite)
- [Microsoft.Extensions.Http](https://www.nuget.org/packages/Microsoft.Extensions.Http)
- Microsoft.Extensions.Logging abstractions (used throughout toolkit components)

## Use
The project is published to [NuGet](https://www.nuget.org/packages/RunnethOverStudio.AppToolkit).

## Versioning
This project uses [Semantic Versioning](https://semver.org/).

- **MAJOR** version: Incompatible API changes
- **MINOR** version: Backward-compatible functionality
- **PATCH** version: Backward-compatible bug fixes

## Build Requirements
- All projects target the LTS version of the [.NET SDK](https://dotnet.microsoft.com/en-us/download).
- The Build project uses [Cake](https://cakebuild.net/) (C# Make) as the build orchestrator and can be launched from your IDE or via script.

	- On OSX/Linux run:
	```bash
	./build.sh
	```
	- If you get a "Permission denied" error, you may need to make the script executable first:
	```bash
	chmod +x build.sh
	```

	- On Windows PowerShell run:
	```powershell
	./build.ps1
	```
