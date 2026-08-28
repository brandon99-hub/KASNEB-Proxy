using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BcProxy.Services;

/// <summary>
/// Industry-standard OData pagination helper.
///
/// Business Central (and most OData v4 servers) enforce a server-side page size
/// (typically 20,000 rows). When a result set is truncated, the response includes
/// an "@odata.nextLink" property containing the URL to retrieve the next page.
///
/// This fetcher follows those continuation links recursively until the server
/// stops returning a nextLink, guaranteeing that ALL records are retrieved
/// regardless of the total dataset size.
///
/// Pattern: https://docs.oasis-open.org/odata/odata/v4.0/errata03/os/complete/
///          part1-protocol/odata-v4.0-errata03-os-part1-protocol-complete.html#_Toc453752292
/// </summary>
public class ODataFetcher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ODataFetcher> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ODataFetcher(HttpClient httpClient, ILogger<ODataFetcher> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// Fetches ALL records from an OData endpoint by following @odata.nextLink
    /// continuation tokens until exhausted.
    /// </summary>
    /// <typeparam name="T">The target entity type to deserialise into</typeparam>
    /// <param name="initialUrl">The first request URL (relative to HttpClient.BaseAddress)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete list of all records across all pages</returns>
    public async Task<List<T>> FetchAllAsync<T>(
        string initialUrl,
        CancellationToken cancellationToken = default)
    {
        var allRecords = new List<T>();
        string? nextUrl = initialUrl;
        int page = 1;

        while (nextUrl is not null)
        {
            _logger.LogDebug("ODataFetcher: fetching page {Page} from {Url}", page, nextUrl);

            var response = await _httpClient.GetAsync(nextUrl, cancellationToken);
            await EnsureSuccessAsync(response);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var odataResult = JsonSerializer.Deserialize<ODataPageResponse<T>>(content, _jsonOptions);

            if (odataResult?.Value is { Count: > 0 })
            {
                allRecords.AddRange(odataResult.Value);
                _logger.LogDebug("ODataFetcher: page {Page} returned {Count} records (total so far: {Total})",
                    page, odataResult.Value.Count, allRecords.Count);
            }

            // @odata.nextLink is an absolute URL when present. We use it directly.
            nextUrl = odataResult?.NextLink;
            page++;
        }

        _logger.LogInformation("ODataFetcher: completed — {TotalRecords} total records fetched in {Pages} page(s)",
            allRecords.Count, page - 1);

        return allRecords;
    }

    // ─── Private Helpers ────────────────────────────────────────────────────────

    private async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Business Central returned {StatusCode}: {Error}",
                response.StatusCode, errorContent);

            throw new BusinessCentralException(
                $"Business Central API returned {response.StatusCode}: {errorContent}",
                response.StatusCode);
        }
    }

    // OData response envelope supporting both "value" (records) and "@odata.nextLink" (pagination)
    private class ODataPageResponse<T>
    {
        [JsonPropertyName("@odata.context")]
        public string? ODataContext { get; set; }

        [JsonPropertyName("value")]
        public List<T>? Value { get; set; }

        /// <summary>
        /// Absolute URL to the next page. Null when this is the final page.
        /// BC emits this as "@odata.nextLink".
        /// </summary>
        [JsonPropertyName("@odata.nextLink")]
        public string? NextLink { get; set; }
    }
}
