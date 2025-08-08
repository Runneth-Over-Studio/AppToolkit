using RunnethOverStudio.AppToolkit.Core;

namespace RunnethOverStudio.AppToolkit.Modules.DataAccess;

/// <summary>
/// Provides utility methods for interacting with an operating system's files and directories.
/// </summary>
public interface IFileSystemAccess
{
    /// <summary>
    /// Gets the path to the application's local data directory, creating it if it does not exist.
    /// </summary>
    /// <returns>
    /// A <see cref="ProcessResult{T}"/> whose <c>Content</c> property contains the full path to the application's local data directory.
    /// </returns>
    ProcessResult<string> GetAppDirectoryPath();

    /// <summary>
    /// Deletes the specified file if it exists, handling read-only files and logging warnings or errors as appropriate.
    /// </summary>
    /// <param name="fullFilePath">The full path of the file to delete.</param>
    /// <returns>
    /// A <see cref="ProcessResult{T}"/> whose <c>Content</c> property is <c>true</c> if the file was deleted; otherwise, <c>false</c>.
    /// </returns>
    ProcessResult<bool> DeleteFile(string fullFilePath);
}
