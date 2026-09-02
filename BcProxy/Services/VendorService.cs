using BcProxy.Models;
using Microsoft.Extensions.Caching.Memory;

namespace BcProxy.Services;

/// <summary>
/// Service for querying Vendor data from Business Central using VendorCard and SuppliersCategories entities.
/// </summary>
public class VendorService
{
    private readonly ODataFetcher _fetcher;
    private readonly ILogger<VendorService> _logger;
    private readonly IMemoryCache _cache;
    private readonly string _vendorCardEntity;
    private readonly string _suppliersCategoriesEntity;

    private const string CategoriesCacheKey = "SupplierCategories_Lookup";

    public VendorService(
        ODataFetcher fetcher,
        ILogger<VendorService> logger,
        IMemoryCache cache,
        IConfiguration configuration)
    {
        _fetcher = fetcher;
        _logger = logger;
        _cache = cache;
        _vendorCardEntity = configuration["BusinessCentral:VendorCardEntity"]
            ?? configuration["BusinessCentral:VendorListEntity"]
            ?? "VendorCard";
        _suppliersCategoriesEntity = configuration["BusinessCentral:SuppliersCategoriesEntity"]
            ?? "SuppliersCategories";
    }

    /// <summary>
    /// Retrieves a dictionary mapping Category_Code -> Description, cached in-memory for 1 hour.
    /// </summary>
    public async Task<Dictionary<string, string>> GetCategoryLookupAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CategoriesCacheKey, out Dictionary<string, string>? cached) && cached != null)
        {
            return cached;
        }

        try
        {
            _logger.LogInformation("Fetching supplier categories from entity '{Entity}'", _suppliersCategoriesEntity);
            var records = await _fetcher.FetchAllAsync<SupplierCategoryRecord>(_suppliersCategoriesEntity, cancellationToken);
            var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var r in records)
            {
                if (!string.IsNullOrWhiteSpace(r.CategoryCode) && !string.IsNullOrWhiteSpace(r.Description))
                {
                    lookup[r.CategoryCode.Trim()] = r.Description.Trim();
                }
            }

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            _cache.Set(CategoriesCacheKey, lookup, cacheOptions);
            _logger.LogInformation("Loaded and cached {Count} supplier categories", lookup.Count);
            return lookup;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch SuppliersCategories from Business Central");
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Returns the full list of supplier categories.
    /// </summary>
    public async Task<List<SupplierCategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var records = await _fetcher.FetchAllAsync<SupplierCategoryRecord>(_suppliersCategoriesEntity, cancellationToken);
            return records
                .Where(r => !string.IsNullOrWhiteSpace(r.CategoryCode))
                .Select(r => new SupplierCategoryDto
                {
                    Code = (r.CategoryCode ?? string.Empty).Trim(),
                    Description = (r.Description ?? string.Empty).Trim(),
                    NoPrequalified = r.NoPrequalified ?? 0
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all supplier categories");
            var lookup = await GetCategoryLookupAsync(cancellationToken);
            return lookup.Select(kv => new SupplierCategoryDto
            {
                Code = kv.Key,
                Description = kv.Value
            }).ToList();
        }
    }

    /// <summary>
    /// Returns a paginated list of vendors from Business Central VendorCard.
    /// Supports filtering by name (contains), vendor code (exact), vendor type (exact), and category code (exact).
    /// </summary>
    public async Task<PagedResponse<VendorDto>> GetVendorsPagedAsync(
        int page = 1,
        int pageSize = 100,
        string? name = null,
        string? no = null,
        string? vendorType = null,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 50;
        if (pageSize > 1000) pageSize = 1000;

        int skip = (page - 1) * pageSize;

        _logger.LogInformation("Fetching vendors from {Entity} page {Page} (pageSize: {PageSize}, skip: {Skip}) — name: '{Name}', no: '{No}', vendorType: '{VendorType}', category: '{Category}'",
            _vendorCardEntity, page, pageSize, skip, name, no, vendorType, category);

        var filterConditions = new List<string>();

        if (!string.IsNullOrWhiteSpace(name))
            filterConditions.Add($"contains(Name, '{ODataEscape(name)}')");

        if (!string.IsNullOrWhiteSpace(no))
            filterConditions.Add($"No eq '{ODataEscape(no)}'");

        if (!string.IsNullOrWhiteSpace(vendorType))
            filterConditions.Add($"Vendor_Type eq '{ODataEscape(vendorType)}'");

        if (!string.IsNullOrWhiteSpace(category))
            filterConditions.Add($"Supplier_Category eq '{ODataEscape(category)}'");

        var queryParams = new List<string>
        {
            $"$top={pageSize}",
            $"$skip={skip}"
        };

        if (filterConditions.Count > 0)
        {
            queryParams.Add($"$filter={string.Join(" and ", filterConditions)}");
        }

        var url = $"{_vendorCardEntity}?{string.Join("&", queryParams)}";
        var records = await _fetcher.FetchSinglePageAsync<VendorCardRecord>(url, cancellationToken);

        _logger.LogInformation("{Entity}: {Count} records returned for page {Page}", _vendorCardEntity, records.Count, page);

        var categoryLookup = await GetCategoryLookupAsync(cancellationToken);
        var items = records.Select(r => MapToDto(r, categoryLookup)).ToList();

        return new PagedResponse<VendorDto>
        {
            Page = page,
            PageSize = pageSize,
            Count = items.Count,
            Data = items
        };
    }

    /// <summary>
    /// Returns a single vendor by vendor code / No (e.g. "00066") with interpreted supplier category.
    /// </summary>
    public async Task<VendorDto?> GetVendorByNoAsync(
        string no,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching vendor by No: {No} from {Entity}", no, _vendorCardEntity);

        var filter = $"No eq '{ODataEscape(no)}'";
        var url = $"{_vendorCardEntity}?$filter={filter}";

        var records = await _fetcher.FetchSinglePageAsync<VendorCardRecord>(url, cancellationToken);
        var record = records.FirstOrDefault();

        if (record is null)
        {
            _logger.LogWarning("No record found in {Entity} for No: {No}", _vendorCardEntity, no);
            return null;
        }

        var categoryLookup = await GetCategoryLookupAsync(cancellationToken);
        return MapToDto(record, categoryLookup);
    }

    /// <summary>
    /// Fetches all vendors from Business Central following @odata.nextLink continuation tokens.
    /// </summary>
    public async Task<List<VendorDto>> GetAllVendorsAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all vendors from {Entity}", _vendorCardEntity);

        var records = await _fetcher.FetchAllAsync<VendorCardRecord>(_vendorCardEntity, cancellationToken);
        var categoryLookup = await GetCategoryLookupAsync(cancellationToken);
        return records.Select(r => MapToDto(r, categoryLookup)).ToList();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Mapping & Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static VendorDto MapToDto(VendorCardRecord r, Dictionary<string, string> categoryLookup)
    {
        var categoryCode = (r.SupplierCategory ?? string.Empty).Trim();
        string categoryDesc = string.Empty;

        if (!string.IsNullOrEmpty(categoryCode))
        {
            categoryLookup.TryGetValue(categoryCode, out var desc);
            categoryDesc = desc ?? categoryCode;
        }

        return new VendorDto
        {
            No = r.No ?? string.Empty,
            Name = r.Name ?? string.Empty,
            SearchName = r.SearchName ?? string.Empty,
            Contact = r.Contact ?? string.Empty,
            PhoneNo = r.PhoneNo ?? string.Empty,
            Email = r.EMail ?? string.Empty,
            FaxNo = r.FaxNo ?? string.Empty,
            ResponsibilityCenter = r.ResponsibilityCenter ?? string.Empty,
            LocationCode = string.Empty,
            PostCode = string.Empty,
            CountryRegionCode = string.Empty,
            VendorType = r.VendorType ?? string.Empty,
            SupplierCategoryCode = categoryCode,
            SupplierCategory = categoryDesc,
            SpecialCategory = r.SpecialCategory ?? string.Empty,
            Address = r.Address ?? string.Empty,
            City = r.City ?? string.Empty,
            County = r.County ?? string.Empty,
            Blocked = r.Blocked ?? string.Empty,
            PrivacyBlocked = r.PrivacyBlocked,
            LastDateModified = r.LastDateModified ?? string.Empty
        };
    }

    private static string ODataEscape(string value) => value.Replace("'", "''");
}
