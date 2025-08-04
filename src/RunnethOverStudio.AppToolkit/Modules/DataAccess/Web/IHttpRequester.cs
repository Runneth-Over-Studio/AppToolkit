using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RunnethOverStudio.AppToolkit.Modules.DataAccess;

/// <summary>
/// Provides methods for making HTTP requests, downloading files, and retrieving web resources.
/// </summary>
public interface IHttpRequester
{
    /// <summary>
    /// Sends an HTTP request to the specified URL and returns the response as a JSON string.
    /// </summary>
    /// <param name="url">The URL to send the request to.</param>
    /// <param name="requestData">Optional request data, including HTTP method, headers, and JSON content.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the response as a string,
    /// or <c>null</c> if the request fails.
    /// </returns>
    Task<string?> SendHTTPJsonRequestAsync(string url, HTTPJsonRequest? requestData = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file from the specified URL to the given file path.
    /// </summary>
    /// <param name="url">The URL of the file to download.</param>
    /// <param name="fullFilePath">The full path where the file will be saved.</param>
    /// <param name="deletePreexisting">If <c>true</c>, deletes any existing file at the target path before downloading.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous download operation.</returns>
    Task DownloadFileAsync(string url, string fullFilePath, bool deletePreexisting = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the response stream from a web request to the specified URL.
    /// </summary>
    /// <param name="url">The URL to request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the response stream,
    /// or an empty <see cref="MemoryStream"/> if the request fails.
    /// </returns>
    Task<Stream> GetWebRequestStreamAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the response content as a byte array from a web request to the specified URL.
    /// </summary>
    /// <param name="url">The URL to request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the response as a byte array,
    /// or an empty array if the request fails.
    /// </returns>
    Task<byte[]> GetWebRequestSerializedAsync(string url, CancellationToken cancellationToken = default);
}
