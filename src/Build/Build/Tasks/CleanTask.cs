using Cake.Common.IO;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;
using System;

namespace Build.Tasks;

[TaskName("Clean")]
[TaskDescription("Deletes the Debug or Release directories in the project bin directories.")]
public sealed class CleanTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        // Find all bin/config directories under src.
        DirectoryPathCollection compileDirs = context.GetDirectories($"{context.SourceDirectory}/**/bin/{context.Config}");

        // Exclude this Build project's own bin/config directory.
        DirectoryPath buildProjectCompileDir = context.MakeAbsolute(context.Directory($"./bin/{context.Config}"));

        foreach (DirectoryPath dir in compileDirs)
        {
            if (!dir.FullPath.Equals(buildProjectCompileDir.FullPath, StringComparison.OrdinalIgnoreCase))
            {
                context.CleanDirectory(dir);
                context.Log.Information($"Cleaned {dir}");
            }
        }
    }
}
