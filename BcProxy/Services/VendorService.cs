using BcProxy.Models;

namespace BcProxy.Services;

/// <summary>
/// Service for querying Vendor data from Business Central.
/// </summary>
public class VendorService
{
    private readonly ODataFetcher _fetcher;
    private readonly ILogger<VendorService> _logger;
    private readonly string _vendorListEntity;

    public VendorService(
        ODataFetcher fetcher,
        ILogger<VendorService> logger,
        IConfiguration configuration)
    {
        _fetcher = fetcher;
        _logger = logger;
        _vendorListEntity = configuration["BusinessCentral:VendorListEntity"] ?? "VendorList";
    }

    /// <summary>
    /// Returns a paginated list of vendors from Business Central.
    /// Supports optional filtering by name (contains), vendor code (exact), or vendor type (exact).
    /// </summary>
    public async Task<PagedResponse<VendorDto>> GetVendorsPagedAsync(
        int page = 1,
        int pageSize = 100,
        string? name = null,
        string? no = null,
        string? vendorType = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 50;
        if (pageSize > 1000) pageSize = 1000;

        int skip = (page - 1) * pageSize;

        _logger.LogInformation("Fetching vendors page {Page} (pageSize: {PageSize}, skip: {Skip}) — name: '{Name}', no: '{No}', vendorType: '{VendorType}'",
            page, pageSize, skip, name, no, vendorType);

        var filterConditions = new List<string>();

        if (!string.IsNullOrWhiteSpace(name))
            filterConditions.Add($"contains(Name, '{ODataEscape(name)}')");

        if (!string.IsNullOrWhiteSpace(no))
            filterConditions.Add($"No eq '{ODataEscape(no)}'");

        if (!string.IsNullOrWhiteSpace(vendorType))
            filterConditions.Add($"Vendor_Type eq '{ODataEscape(vendorType)}'");

        var queryParams = new List<string>
        {
            $"$top={pageSize}",
            $"$skip={skip}"
        };

        if (filterConditions.Count > 0)
        {
            queryParams.Add($"$filter={string.Join(" and ", filterConditions)}");
        }

        var url = $"{_vendorListEntity}?{string.Join("&", queryParams)}";
        var records = await _fetcher.FetchSinglePageAsync<VendorListRecord>(url, cancellationToken);

        _logger.LogInformation("VendorList: {Count} records returned for page {Page}", records.Count, page);

        var items = records.Select(MapToDto).ToList();

        return new PagedResponse<VendorDto>
        {
            Page = page,
            PageSize = pageSize,
            Count = items.Count,
            Data = items
        };
    }

    /// <summary>
    /// Returns a single vendor by its vendor code / No (e.g. "00066").
    /// </summary>
    public async Task<VendorDto?> GetVendorByNoAsync(
        string no,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching vendor by No: {No}", no);

        var filter = $"No eq '{ODataEscape(no)}'";
        var url = $"{_vendorListEntity}?$filter={filter}";

        var records = await _fetcher.FetchSinglePageAsync<VendorListRecord>(url, cancellationToken);
        var record = records.FirstOrDefault();

        if (record is null)
        {
            _logger.LogWarning("No VendorList record found for No: {No}", no);
            return null;
        }

        return MapToDto(record);
    }

    /// <summary>
    /// Fetches all vendors from Business Central by following @odata.nextLink continuation tokens.
    /// </summary>
    public async Task<List<VendorDto>> GetAllVendorsAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all vendors across all pages");

        var records = await _fetcher.FetchAllAsync<VendorListRecord>(_vendorListEntity, cancellationToken);
        return records.Select(MapToDto).ToList();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Mapping & Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static VendorDto MapToDto(VendorListRecord r)
    {
        // Fall back to E_Mail if Email is empty or whitespace
        var email = !string.IsNullOrWhiteSpace(r.Email)
            ? r.Email
            : (r.EMail ?? string.Empty);

        return new VendorDto
        {
            No = r.No ?? string.Empty,
            Name = r.Name ?? string.Empty,
            SearchName = r.SearchName ?? string.Empty,
            Contact = r.Contact ?? string.Empty,
            PhoneNo = r.PhoneNo ?? string.Empty,
            Email = email,
            FaxNo = r.FaxNo ?? string.Empty,
            ResponsibilityCenter = r.ResponsibilityCenter ?? string.Empty,
            LocationCode = r.LocationCode ?? string.Empty,
            PostCode = r.PostCode ?? string.Empty,
            CountryRegionCode = r.CountryRegionCode ?? string.Empty,
            VendorType = r.VendorType ?? string.Empty,
            Blocked = r.Blocked ?? string.Empty,
            PrivacyBlocked = r.PrivacyBlocked,
            LastDateModified = r.LastDateModified ?? string.Empty
        };
    }

    private static string ODataEscape(string value) => value.Replace("'", "''");
}
