﻿using Cake.Common;
using Cake.Core.Diagnostics;
using Cake.Frosting;
using System;
using System.Diagnostics;

namespace Build.Tasks;

[TaskName("Linting")]
[IsDependentOn(typeof(RestoreTask))]
[TaskDescription("Applies style preferences and static analysis recommendations to projects.")]
public sealed class LintingTask : FrostingTask<BuildContext>
{
    // ref: https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format
    //      https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/
    //      https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-files#editorconfig

    private const string SOLUTION_NAME = "AppToolkit.sln";

    public override void Run(BuildContext context)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        context.Log.Information($"Formatting solution...");

        string solutionPath = System.IO.Path.Combine(context.SourceDirectory, SOLUTION_NAME);
        //context.StartProcess("dotnet", $"format \"{solutionPath}\" --no-restore --report \"{context.CLIProjectOutputDirectory.Path.FullPath}\"");
        //context.StartProcess("dotnet", $"format \"{solutionPath}\" --no-restore --report \"{context.DesktopProjectOutputDirectory.Path.FullPath}\"");

        foreach (ReleaseProject project in context.ReleaseProjects)
        {
            context.StartProcess("dotnet", $"format \"{solutionPath}\" --no-restore --report \"{project.OutputDirectory.Path.FullPath}\"");
        }

        stopwatch.Stop();
        double completionTime = Math.Round(stopwatch.Elapsed.TotalSeconds, 1);
        context.Log.Information($"Linting complete ({completionTime}s)");
    }
}