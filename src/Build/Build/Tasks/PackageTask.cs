using Cake.Common.IO;
using Cake.Common.IO.Paths;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNet.Pack;
using Cake.Core.IO;
using Cake.Frosting;

using static Build.BuildContext;

namespace Build.Tasks;

[TaskName("Package")]
[IsDependentOn(typeof(PublishTask))]
[TaskDescription("Generates the NuGet packages using previously processed images and project properties.")]
public sealed class PackageTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        return context.Config == BuildConfigurations.Release;
    }

    public override void Run(BuildContext context)
    {
        foreach (ReleaseProject project in context.ReleaseProjects)
        {
            PackageProject(context, project.Name, project.Directory, project.OutputDirectory);
        }
    }

    private static void PackageProject(BuildContext context, string projectName, ConvertableDirectoryPath projectDirectory, ConvertableDirectoryPath outputDirectory)
    {
        string projectPath = projectDirectory + context.File($"{projectName}.csproj");
        DirectoryPath nugetOutputDirectoryPath = outputDirectory + context.Directory("NuGet");

        context.DotNetPack(projectPath, new DotNetPackSettings
        {
            Configuration = context.Config.ToString(),
            NoRestore = true,
            NoBuild = true,
            OutputDirectory = nugetOutputDirectoryPath,
            MSBuildSettings = new DotNetMSBuildSettings().WithProperty("ContinuousIntegrationBuild", "true")
        });
    }
}
