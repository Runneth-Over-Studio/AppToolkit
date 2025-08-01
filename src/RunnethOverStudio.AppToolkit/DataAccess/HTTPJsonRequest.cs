using System.Collections.Generic;

namespace RunnethOverStudio.AppToolkit.DataAccess;

/// <summary>
/// Represents the data required to make an HTTP request with optional JSON content and custom headers.
/// </summary>
public record HTTPJsonRequest
{
    /// <summary>
    /// HTTP method to use for the request (e.g., "GET", "POST", "PUT"). Defaults to "GET".
    /// </summary>
    public string HttpMethod { get; init; } = "GET";

    /// <summary>
    /// Value indicating whether to use a compression-enabled HTTP client for the request.
    /// </summary>
    public bool UseCompression { get; init; }

    /// <summary>
    /// JSON content to include in the request body, if applicable.
    /// </summary>
    public string? JsonContent { get; init; }

    /// <summary>
    /// Value to use for the User-Agent header in the request.
    /// </summary>
    public string? UserAgent { get; init; }

    /// <summary>
    /// Additional request headers to include in the HTTP request.
    /// </summary>
    public Dictionary<string, string>? RequestHeaders { get; init; }
}
