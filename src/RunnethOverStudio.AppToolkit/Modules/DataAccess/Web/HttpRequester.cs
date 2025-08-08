using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace RunnethOverStudio.AppToolkit.Modules.DataAccess;

/// <summary>
/// Provides methods for making HTTP requests, downloading files, and retrieving web resources.
/// </summary>
/// <remarks>
/// This implementation uses <see cref="IHttpClientFactory"/> for efficient and configurable HTTP client management.
/// </remarks>
public sealed class HttpRequester : IHttpRequester
{
    /// <summary>
    /// The named HTTP client to use when compression is required for HTTP requests.
    /// Configure this client in your application's <c>IHttpClientFactory</c> setup to support compression (e.g., GZip, Brotli).
    /// </summary>
    public const string COMPRESSION_CLIENT_NAME = "CompressionClient";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpRequester"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory used to create <see cref="HttpClient"/> instances.</param>
    /// <param name="logger">The logger used for error and status messages.</param>
    public HttpRequester(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
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

            HttpClient client = requestData.UseCompression ? _httpClientFactory.CreateClient(COMPRESSION_CLIENT_NAME) : _httpClientFactory.CreateClient();
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<Stream> GetWebRequestStreamAsync(string url, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage? response = await GetWebRequestResponseAsync(url, cancellationToken);

        if (response != null)
        {
            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }

        return new MemoryStream();
    }

    /// <inheritdoc/>
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
