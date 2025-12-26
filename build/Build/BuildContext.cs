using Build.DTOs;
using Cake.Common.IO;
using Cake.Common.IO.Paths;
using Cake.Common.Xml;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Build;

public sealed class BuildContext : FrostingContext
{
    internal const string REPO_NAME = "AppToolkit";
    internal const string LOGO_SVG_FILENAME = "logo.svg";

    public enum BuildConfigurations
    {
        Debug,
        Release
    }

    public BuildConfigurations Config { get; }
    public JsonSerializerOptions SerializerOptions { get; }
    public string AbsolutePathToRepo { get; }
    public ConvertableDirectoryPath SourceDirectory { get; }
    public List<ReleaseProject> ReleaseProjects { get; }

    public BuildContext(ICakeContext context) : base(context)
    {
        string configArgument = context.Arguments.GetArgument("Configuration") ?? string.Empty;
        Config = configArgument.ToLower() switch
        {
            "release" => BuildConfigurations.Release,
            _ => BuildConfigurations.Debug,
        };

        SerializerOptions = new() { PropertyNameCaseInsensitive = true };
        AbsolutePathToRepo = GetRepoAbsolutePath(REPO_NAME, this);
        SourceDirectory = AbsolutePathToRepo + context.Directory("src");

        ConvertableFilePath buildProjectFilePath = context.Directory("./") + context.File("Build.csproj"); // All projects should have the same target framework.
        string targetFramework = context.XmlPeek(buildProjectFilePath, "/Project/PropertyGroup/TargetFramework");

        ReleaseProjects =
        [
            SetReleaseProject(this, targetFramework, "RunnethOverStudio.AppToolkit")
        ];
    }

    private static string GetRepoAbsolutePath(string repoName, ICakeContext context)
    {
        // Start from the working directory.
        DirectoryPath dir = context.Environment.WorkingDirectory;

        // Traverse up until we find the directory named after the repository name.
        while (dir != null && !dir.GetDirectoryName().Equals(repoName, StringComparison.OrdinalIgnoreCase))
        {
            dir = dir.GetParent();
        }

        if (dir == null)
        {
            throw new InvalidOperationException($"Could not find repository root directory named '{repoName}' in parent chain.");
        }

        return dir.FullPath;
    }

    private static ReleaseProject SetReleaseProject(BuildContext context, string targetFramework, string fullProjectName, bool isSdkStyleProject = true)
    {
        ConvertableDirectoryPath baseProjectDirectory = context.SourceDirectory + context.Directory(fullProjectName);

        return new ReleaseProject
        {
            Name = fullProjectName,
            DirectoryPathAbsolute = baseProjectDirectory,
            CsprojFilePathAbsolute = baseProjectDirectory + context.File($"{fullProjectName}.csproj"),
            OutputDirectoryPathAbsolute = baseProjectDirectory + context.Directory($"bin/{context.Config}/{targetFramework}"),
            IsSdkStyleProject = isSdkStyleProject
        };
    }
}
