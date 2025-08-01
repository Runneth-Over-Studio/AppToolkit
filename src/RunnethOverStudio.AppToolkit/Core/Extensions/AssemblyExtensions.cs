using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RunnethOverStudio.AppToolkit.Core.Extensions;

/// <summary>
/// Provides extension methods for working with <see cref="Assembly"/> instances.
/// </summary>
public static class AssemblyExtensions
{
    /// <summary>
    /// Reads the contents of an embedded resource from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly containing the embedded resource.</param>
    /// <param name="name">The name (or ending segment) of the resource to read.</param>
    /// <returns>
    /// The contents of the embedded resource as a string, or <c>null</c> if the resource is not found.
    /// </returns>
    /// <remarks>
    /// Inspired by <see href="https://stackoverflow.com/a/3314213">this answer</see> by dtb.
    /// </remarks>
    public static string? ReadResource(this Assembly assembly, string name)
    {
        using Stream? stream = GetAssemblyResourceStream(assembly, name);

        if (stream != null)
        {
            using StreamReader reader = new(stream);
            return reader.ReadToEnd();
        }

        return null;
    }

    /// <summary>
    /// Asynchronously reads the contents of an embedded resource from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly containing the embedded resource.</param>
    /// <param name="name">The name (or ending segment) of the resource to read.</param>
    /// <returns>
    /// The contents of the embedded resource as a string, or <c>null</c> if the resource is not found.
    /// </returns>
    /// <remarks>
    /// Inspired by <see href="https://stackoverflow.com/a/3314213">this answer</see> by dtb and Mikael Dúi Bolinder.
    /// </remarks>
    public static async Task<string?> ReadResourceAsync(this Assembly assembly, string name)
    {
        using Stream? stream = GetAssemblyResourceStream(assembly, name);

        if (stream != null)
        {
            using StreamReader reader = new(stream);
            return await reader.ReadToEndAsync();
        }

        return null;
    }

    private static Stream? GetAssemblyResourceStream(Assembly assembly, string name)
    {
        string resourcePath = assembly
            .GetManifestResourceNames()
            .Single(str => str.EndsWith(name));

        return assembly.GetManifestResourceStream(resourcePath);
    }
}