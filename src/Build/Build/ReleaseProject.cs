using Cake.Common.IO.Paths;

namespace Build;

public record ReleaseProject
{
    public required string Name { get; init; }

    public required ConvertableDirectoryPath Directory { get; init; }

    public required string FilePath { get; init; }

    public required ConvertableDirectoryPath OutputDirectory { get; init; }
}
