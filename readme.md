<p align="left">
  <img src="https://raw.githubusercontent.com/Runneth-Over-Studio/AppToolkit/refs/heads/main/content/logo.png" width="175" alt="App Toolkit Logo">
</p>

# App Toolkit
Runneth Over Studio's common C# app development code.

## Purpose
This library is designed to accelerate the development of prototypes, in-house utilities, and narrowly focused applications. It is not intended for use in large-scale, robust enterprise systems. The primary goal is to enable rapid domain-specific feature development by providing reusable components and eliminating the need to maintain boilerplate code across multiple projects. By leveraging this library, developers can focus on delivering core functionality without being burdened by repetitive infrastructure concerns.

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
