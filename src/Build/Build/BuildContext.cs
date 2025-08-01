using Cake.Common.IO;
using Cake.Common.IO.Paths;
using Cake.Common.Xml;
using Cake.Core;
using Cake.Frosting;
using System.Collections.Generic;
using System.Text.Json;

namespace Build;

public sealed class BuildContext : FrostingContext
{
    public enum BuildConfigurations
    {
        Debug,
        Release
    }

    public BuildConfigurations Config { get; }
    public JsonSerializerOptions SerializerOptions { get; }
    public ConvertableDirectoryPath RootDirectory { get; }
    public ConvertableDirectoryPath SourceDirectory { get; }
    public string TargetFramework { get; }
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
        RootDirectory = context.Directory("../../../");
        SourceDirectory = RootDirectory + context.Directory("src");

        ConvertableFilePath buildProjectFilePath = context.Directory("./") + context.File("Build.csproj"); // All projects should have the same target framework.
        TargetFramework = context.XmlPeek(buildProjectFilePath, "/Project/PropertyGroup/TargetFramework");

        ReleaseProjects =
        [
            SetReleaseProject(this, string.Empty),
            SetReleaseProject(this, "CLI"),
            SetReleaseProject(this, "Desktop")
        ];
    }

    private static ReleaseProject SetReleaseProject(BuildContext context, string projectExtension)
    {
        string baseProjectName = "RunnethOverStudio.AppToolkit";
        if (!string.IsNullOrEmpty(projectExtension))
        {
            baseProjectName += $".{projectExtension}";
        }

        ConvertableDirectoryPath baseProjectDirectory = context.SourceDirectory + context.Directory(baseProjectName);

        return new ReleaseProject
        {
            Name = baseProjectName,
            Directory = baseProjectDirectory,
            FilePath = baseProjectDirectory + context.File($"{baseProjectName}.csproj"),
            OutputDirectory = baseProjectDirectory + context.Directory($"bin/{context.Config}/{context.TargetFramework}")
        };
    }
}
