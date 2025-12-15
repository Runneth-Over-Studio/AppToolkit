using Build.DTOs;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Frosting;
using static Build.BuildContext;

namespace Build.Tasks.Standard;

[TaskName("Publish")]
[IsDependentOn(typeof(CompileProjectsTask))]
[TaskDescription("Publishes projects using the Release configuration, applying publish settings defined in their .csproj files.")]
public sealed class PublishTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        return context.Config == BuildConfigurations.Release;
    }

    public override void Run(BuildContext context)
    {
        foreach (ReleaseProject project in context.ReleaseProjects)
        {
            PublishProject(context, project.CsprojFilePathAbsolute);
        }
    }

    private static void PublishProject(BuildContext context, string projectPath)
    {
        context.DotNetPublish(projectPath, new DotNetPublishSettings
        {
            Configuration = context.Config.ToString()
        });
    }
}