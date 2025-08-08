using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace RunnethOverStudio.AppToolkit.Modules.DataAccess;

/// <summary>
/// Provides utility methods for operating system file and directory operations.
/// </summary>
public static class AppFileSystem
{
    /// <summary>
    /// Gets the path to the application's local data directory, creating it if it does not exist.
    /// </summary>
    /// <returns>
    /// The full path to the application's local data directory.
    /// </returns>
    public static string GetAppDirectoryPath()
    {
        string localAppDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string thisAppName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
        string appDirectory = Path.Combine(localAppDirectory, thisAppName);

        Directory.CreateDirectory(appDirectory);

        return appDirectory;
    }

    /// <summary>
    /// Deletes the specified file if it exists, handling read-only files and logging warnings or errors as appropriate.
    /// </summary>
    /// <param name="fullFilePath">The full path of the file to delete.</param>
    /// <param name="logger">The logger to use for warning and error messages. Optional.</param>
    /// <returns><c>true</c> if the file was deleted; otherwise, <c>false</c>.</returns>
    public static bool DeleteFile(string fullFilePath, ILogger? logger = null)
    {
        try
        {
            if (File.Exists(fullFilePath))
            {
                FileAttributes attributes = File.GetAttributes(fullFilePath);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    // Remove read-only attribute before deleting.
                    File.SetAttributes(fullFilePath, attributes & ~FileAttributes.ReadOnly);
                    logger?.LogInformation("Removed read-only attribute from file: {FilePath}", fullFilePath);
                }

                File.Delete(fullFilePath);
                return true;
            }
            else
            {
                logger?.LogWarning("Attempted to delete a file that does not exist: {FilePath}", fullFilePath);
                return false;
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to delete file: {FilePath}", fullFilePath);
            return false;
        }
    }
}
