using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace RunnethOverStudio.AppToolkit.DataAccess;

/// <summary>
/// Provides methods for making HTTP requests, downloading files, and retrieving web resources.
/// </summary>
public sealed class WebRequester
{
    /// <summary>
    /// The named HTTP client to use when compression is required for HTTP requests.
    /// Configure this client in your application's <c>IHttpClientFactory</c> setup to support compression (e.g., GZip, Brotli).
    /// </summary>
    public const string COMPRESSION_CLIENT_NAME = "CompressionClient";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebRequester"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory used to create <see cref="HttpClient"/> instances.</param>
    /// <param name="logger">The logger used for error and status messages.</param>
    public WebRequester(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

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
    public async Task<string?> SendHTTPJsonRequestAsync(string url, HTTPJsonRequest? requestData = null, CancellationToken cancellationToken = default)
    {
        if (requestData == null)
        {
            try
            {
                HttpClient defaultClient = _httpClientFactory.CreateClient();
                defaultClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
                return await defaultClient.GetStringAsync(url, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Standard HTTP request failed.");
                return null;
            }
        }

        try
        {
            using HttpRequestMessage request = new(new HttpMethod(requestData.HttpMethod), url);

            if (requestData.RequestHeaders != null)
            {
                request.Headers.TryAddWithoutValidation("Accept", "application/json");

                foreach (KeyValuePair<string, string> valueByName in requestData.RequestHeaders)
                {
                    request.Headers.TryAddWithoutValidation(valueByName.Key, valueByName.Value);
                }
            }

            if (requestData.UserAgent != null)
            {
                request.Headers.UserAgent.ParseAdd(requestData.UserAgent);
            }

            if (!string.IsNullOrEmpty(requestData.JsonContent))
            {
                request.Content = new StringContent(requestData.JsonContent);
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            }

            HttpClient client = (requestData.UseCompression ? _httpClientFactory.CreateClient(COMPRESSION_CLIENT_NAME) : _httpClientFactory.CreateClient());
            using HttpResponseMessage httpResponse = await client.SendAsync(request, cancellationToken);
            string response = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Response message HTTP request failed.");
            return null;
        }
    }

    /// <summary>
    /// Downloads a file from the specified URL to the given file path.
    /// </summary>
    /// <param name="url">The URL of the file to download.</param>
    /// <param name="fullFilePath">The full path where the file will be saved.</param>
    /// <param name="deletePreexisting">If <c>true</c>, deletes any existing file at the target path before downloading.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous download operation.</returns>
    public async Task DownloadFileAsync(string url, string fullFilePath, bool deletePreexisting = false, CancellationToken cancellationToken = default)
    {
        if (deletePreexisting && File.Exists(fullFilePath))
        {
            File.Delete(fullFilePath);
        }

        string tempFilePath = fullFilePath + ".download";
        HttpClient client = _httpClientFactory.CreateClient();

        try
        {
            using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            await using (var httpStream = await response.Content.ReadAsStreamAsync(cancellationToken))
            {
                await httpStream.CopyToAsync(fileStream, cancellationToken);
            }

            File.Move(tempFilePath, fullFilePath, overwrite: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File download failed.");
            if (File.Exists(tempFilePath))
            {
                try { File.Delete(tempFilePath); } catch { /* ignore */ }
            }
            throw;
        }
    }

    /// <summary>
    /// Retrieves the response stream from a web request to the specified URL.
    /// </summary>
    /// <param name="url">The URL to request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the response stream,
    /// or an empty <see cref="MemoryStream"/> if the request fails.
    /// </returns>
    public async Task<Stream> GetWebRequestStreamAsync(string url, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage? response = await GetWebRequestResponseAsync(url, cancellationToken);

        if (response != null)
        {
            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }

        return new MemoryStream();
    }

    /// <summary>
    /// Retrieves the response content as a byte array from a web request to the specified URL.
    /// </summary>
    /// <param name="url">The URL to request.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the response as a byte array,
    /// or an empty array if the request fails.
    /// </returns>
    public async Task<byte[]> GetWebRequestSerializedAsync(string url, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage? response = await GetWebRequestResponseAsync(url, cancellationToken);

        if (response != null)
        {
            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }

        return [];
    }

    private async Task<HttpResponseMessage?> GetWebRequestResponseAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            HttpClient client = _httpClientFactory.CreateClient();
            return await client.GetAsync(url, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Web request failed.");
            return null;
        }
    }
}
