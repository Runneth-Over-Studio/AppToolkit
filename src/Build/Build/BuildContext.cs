using Cake.Common.IO;
using Cake.Common.IO.Paths;
using Cake.Common.Xml;
using Cake.Core;
using Cake.Frosting;
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
    public string TargetFramework { get; }
    public string CLIProjectName { get; }
    public string DesktopProjectName { get; }
    public ConvertableDirectoryPath RootDirectory { get; }
    public ConvertableDirectoryPath SourceDirectory { get; }
    public ConvertableDirectoryPath CLIProjectDirectory { get; }
    public ConvertableDirectoryPath DesktopProjectDirectory { get; }
    public ConvertableDirectoryPath CLIProjectOutputDirectory { get; }
    public ConvertableDirectoryPath DesktopProjectOutputDirectory { get; }

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

        CLIProjectName = "RunnethOverStudio.AppToolkit.CLI";
        CLIProjectDirectory = SourceDirectory + context.Directory(CLIProjectName);
        CLIProjectOutputDirectory = CLIProjectDirectory + context.Directory($"bin/{Config}/{TargetFramework}");

        DesktopProjectName = "RunnethOverStudio.AppToolkit.Desktop";
        DesktopProjectDirectory = SourceDirectory + context.Directory(DesktopProjectName);
        DesktopProjectOutputDirectory = DesktopProjectDirectory + context.Directory($"bin/{Config}/{TargetFramework}");

        TargetFramework = context.XmlPeek(CLIProjectDirectory + context.File($"{CLIProjectName}.csproj"), "/Project/PropertyGroup/TargetFramework");
    }
}
