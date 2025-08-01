using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace RunnethOverStudio.AppToolkit.Core.Extensions;

/// <summary>
/// Provides extension methods for working with <see cref="HttpClient"/> instances.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Asynchronously downloads the resource at the specified URI to a local file.
    /// </summary>
    /// <param name="client">The <see cref="HttpClient"/> instance used to perform the download.</param>
    /// <param name="address">The URI of the resource to download.</param>
    /// <param name="fileName">The full path of the file to which the resource will be saved.</param>
    /// <remarks>
    /// Inspired by <see href="https://stackoverflow.com/a/66270371">this answer</see> by Tony.
    /// </remarks>
    public static async Task DownloadFileTaskAsync(this HttpClient client, Uri address, string fileName)
    {
        using Stream stream = await client.GetStreamAsync(address);
        using FileStream fileStream = new(fileName, FileMode.CreateNew);
        await stream.CopyToAsync(fileStream);
    }
}
