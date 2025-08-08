using Microsoft.Extensions.Logging;
using RunnethOverStudio.AppToolkit.Core;
using System;
using System.IO;
using static RunnethOverStudio.AppToolkit.Core.Enums;

namespace RunnethOverStudio.AppToolkit.Modules.DataAccess;

/// <summary>
/// Provides utility methods for interacting with an operating system's files and directories.
/// </summary>
public sealed class FileSystemAccess : IFileSystemAccess
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemAccess"/> class with the specified logger.
    /// </summary>
    /// <param name="logger">The logger used to record informational messages, warnings, and errors related to file system operations.</param>
    public FileSystemAccess(ILogger<IFileSystemAccess> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public ProcessResult<string> GetAppDirectoryPath()
    {
        try
        {
            string localAppDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string thisAppName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
            string appDirectory = Path.Combine(localAppDirectory, thisAppName);

            Directory.CreateDirectory(appDirectory);

            return ProcessResult<string>.Success(appDirectory);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get or create application directory.");
            return ProcessResult<string>.Failure(string.Empty, StatusCodes.InternalError, ex);
        }
    }

    /// <inheritdoc/>
    public ProcessResult<bool> DeleteFile(string fullFilePath)
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
                    _logger?.LogInformation("Removed read-only attribute from file: {FilePath}", fullFilePath);
                }

                File.Delete(fullFilePath);
                return ProcessResult<bool>.Success(true);
            }
            else
            {
                FileNotFoundException ex = new("Attempted to delete a file that does not exist.", fullFilePath);
                _logger?.LogWarning(ex, "Nothing to delete.");
                return ProcessResult<bool>.Failure(false, StatusCodes.NotFound, ex);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to delete file: {FilePath}", fullFilePath);
            return ProcessResult<bool>.Failure(false, StatusCodes.InternalError, ex);
        }
    }
}
