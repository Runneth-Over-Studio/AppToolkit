using Microsoft.Extensions.Logging;
using RunnethOverStudio.AppToolkit.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
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
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the response as a string,
    /// or <c>null</c> if the request fails.
    /// </returns>
    public async Task<string?> SendHTTPJsonRequestAsync(string url, HTTPJsonRequest? requestData = null)
    {
        try
        {
            if (requestData == null)
            {
                HttpClient defaultClient = _httpClientFactory.CreateClient();
                defaultClient.DefaultRequestHeaders.Add("Accept", "application/json");

                return await defaultClient.GetStringAsync(url);
            }

            HttpClient client = (requestData.UseCompression ? _httpClientFactory.CreateClient(COMPRESSION_CLIENT_NAME) : _httpClientFactory.CreateClient());
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            if (requestData.UserAgent != null)
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(requestData.UserAgent);
            }

            if (requestData.RequestHeaders != null)
            {
                foreach (KeyValuePair<string, string> valueByName in requestData.RequestHeaders)
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(valueByName.Key, valueByName.Value);
                }
            }

            if (string.IsNullOrEmpty(requestData.JsonContent) && string.Equals(requestData.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                string response = await client.GetStringAsync(url);
                return response;
            }

            using (HttpRequestMessage request = new(new HttpMethod(requestData.HttpMethod), url))
            {
                if (!string.IsNullOrEmpty(requestData.JsonContent))
                {
                    request.Content = new StringContent(requestData.JsonContent);
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                }

                using HttpResponseMessage httpResponse = await client.SendAsync(request);
                string response = await httpResponse.Content.ReadAsStringAsync();
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP request failed.");
            return null;
        }
    }

    /// <summary>
    /// Downloads a file from the specified URL to the given file path.
    /// </summary>
    /// <param name="url">The URL of the file to download.</param>
    /// <param name="fullFilePath">The full path where the file will be saved.</param>
    /// <param name="deletePreexisting">If <c>true</c>, deletes any existing file at the target path before downloading.</param>
    /// <returns>A task representing the asynchronous download operation.</returns>
    public async Task DownloadFileAsync(string url, string fullFilePath, bool deletePreexisting = false)
    {
        if (deletePreexisting && File.Exists(fullFilePath))
        {
            File.Delete(fullFilePath);
        }

        HttpClient client = _httpClientFactory.CreateClient();
        await client.DownloadFileTaskAsync(new Uri(url), fullFilePath);
    }

    /// <summary>
    /// Retrieves the response stream from a web request to the specified URL.
    /// </summary>
    /// <param name="url">The URL to request.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the response stream,
    /// or an empty <see cref="MemoryStream"/> if the request fails.
    /// </returns>
    public async Task<Stream> GetWebRequestStreamAsync(string url)
    {
        using HttpResponseMessage? response = await GetWebRequestResponseAsync(url);

        if (response != null)
        {
            return await response.Content.ReadAsStreamAsync();

        }

        return new MemoryStream();
    }

    /// <summary>
    /// Retrieves the response content as a byte array from a web request to the specified URL.
    /// </summary>
    /// <param name="url">The URL to request.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the response as a byte array,
    /// or an empty array if the request fails.
    /// </returns>
    public async Task<byte[]> GetWebRequestSerializedAsync(string url)
    {
        using HttpResponseMessage? response = await GetWebRequestResponseAsync(url);

        if (response != null)
        {
            return await response.Content.ReadAsByteArrayAsync();
        }

        return [];
    }

    private async Task<HttpResponseMessage?> GetWebRequestResponseAsync(string url)
    {
        try
        {
            HttpClient client = _httpClientFactory.CreateClient();
            return await client.GetAsync(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Web request failed.");
            return null;
        }
    }
}
