using BcProxy.Models;
using Microsoft.Extensions.Caching.Memory;

namespace BcProxy.Services;

/// <summary>
/// Service for querying Exam Centers, Exam Zones, and Exam Regions from Business Central ERP.
/// Enriches center records with geographic zone names and regional descriptions, and flags
/// centers that also serve as training institutions.
/// </summary>
public class ExamCenterService
{
    private readonly ODataFetcher _fetcher;
    private readonly ILogger<ExamCenterService> _logger;
    private readonly IMemoryCache _cache;
    private readonly string _examCenterCardEntity;
    private readonly string _examZonesEntity;
    private readonly string _examRegionsEntity;

    private const string ZonesCacheKey = "ExamZones_Lookup";
    private const string RegionsCacheKey = "ExamRegions_Lookup";

    public ExamCenterService(
        ODataFetcher fetcher,
        ILogger<ExamCenterService> logger,
        IMemoryCache cache,
        IConfiguration configuration)
    {
        _fetcher = fetcher;
        _logger = logger;
        _cache = cache;

        _examCenterCardEntity = configuration["BusinessCentral:ExamCenterCardEntity"] ?? "ExamCenterCard";
        _examZonesEntity = configuration["BusinessCentral:ExamZonesEntity"] ?? "ExamZones";
        _examRegionsEntity = configuration["BusinessCentral:ExamRegionsEntity"] ?? "ExamRegions";
    }

    /// <summary>
    /// Retrieves a dictionary mapping Zone Code -> Zone Name, cached in-memory for 1 hour.
    /// </summary>
    public async Task<Dictionary<string, string>> GetZoneLookupAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(ZonesCacheKey, out Dictionary<string, string>? cached) && cached != null)
        {
            return cached;
        }

        try
        {
            _logger.LogInformation("Fetching exam zones from entity '{Entity}'", _examZonesEntity);
            var records = await _fetcher.FetchAllAsync<ExamZoneRecord>(_examZonesEntity, cancellationToken);
            var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var r in records)
            {
                if (!string.IsNullOrWhiteSpace(r.Code) && !string.IsNullOrWhiteSpace(r.ZoneName))
                {
                    lookup[r.Code.Trim()] = r.ZoneName.Trim();
                }
            }

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            _cache.Set(ZonesCacheKey, lookup, cacheOptions);
            _logger.LogInformation("Loaded and cached {Count} exam zones", lookup.Count);
            return lookup;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch ExamZones from Business Central");
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Retrieves a dictionary mapping Region Code -> Description, cached in-memory for 1 hour.
    /// </summary>
    public async Task<Dictionary<string, string>> GetRegionLookupAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(RegionsCacheKey, out Dictionary<string, string>? cached) && cached != null)
        {
            return cached;
        }

        try
        {
            _logger.LogInformation("Fetching exam regions from entity '{Entity}'", _examRegionsEntity);
            var records = await _fetcher.FetchAllAsync<ExamRegionRecord>(_examRegionsEntity, cancellationToken);
            var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var r in records)
            {
                if (!string.IsNullOrWhiteSpace(r.Code) && !string.IsNullOrWhiteSpace(r.Description))
                {
                    lookup[r.Code.Trim()] = r.Description.Trim();
                }
            }

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            _cache.Set(RegionsCacheKey, lookup, cacheOptions);
            _logger.LogInformation("Loaded and cached {Count} exam regions", lookup.Count);
            return lookup;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch ExamRegions from Business Central");
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Returns the full list of Exam Zones.
    /// </summary>
    public async Task<List<ExamZoneDto>> GetAllZonesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var records = await _fetcher.FetchAllAsync<ExamZoneRecord>(_examZonesEntity, cancellationToken);
            return records
                .Where(r => !string.IsNullOrWhiteSpace(r.Code))
                .Select(r => new ExamZoneDto
                {
                    Code = (r.Code ?? string.Empty).Trim(),
                    ZoneName = (r.ZoneName ?? string.Empty).Trim(),
                    RegionCode = (r.RegionCode ?? string.Empty).Trim(),
                    NoOfExamCenters = r.NoOfExamCenters ?? 0
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all exam zones");
            var lookup = await GetZoneLookupAsync(cancellationToken);
            return lookup.Select(kv => new ExamZoneDto
            {
                Code = kv.Key,
                ZoneName = kv.Value
            }).ToList();
        }
    }

    /// <summary>
    /// Returns the full list of Exam Regions.
    /// </summary>
    public async Task<List<ExamRegionDto>> GetAllRegionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var records = await _fetcher.FetchAllAsync<ExamRegionRecord>(_examRegionsEntity, cancellationToken);
            return records
                .Where(r => !string.IsNullOrWhiteSpace(r.Code))
                .Select(r => new ExamRegionDto
                {
                    Code = (r.Code ?? string.Empty).Trim(),
                    Description = (r.Description ?? string.Empty).Trim(),
                    NoOfZones = r.NoOfZones ?? 0,
                    NoOfExamCenters = r.NoOfExamCenters ?? 0,
                    Blocked = r.Blocked
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all exam regions");
            var lookup = await GetRegionLookupAsync(cancellationToken);
            return lookup.Select(kv => new ExamRegionDto
            {
                Code = kv.Key,
                Description = kv.Value
            }).ToList();
        }
    }

    /// <summary>
    /// Returns a paginated list of Exam Centers from Business Central ExamCenterCard.
    /// Supports filtering by name, code, zone, region, type, status, and whether it functions as a training institution.
    /// Enriches all results with human-readable zone and region descriptions.
    /// </summary>
    public async Task<PagedResponse<ExamCenterDto>> GetExamCentersPagedAsync(
        int page = 1,
        int pageSize = 100,
        string? name = null,
        string? code = null,
        string? zone = null,
        string? region = null,
        string? type = null,
        string? status = null,
        bool? isTrainingInstitution = null,
        bool? disabilityFriendly = null,
        bool? computerBasedFriendly = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 50;
        if (pageSize > 1000) pageSize = 1000;

        int skip = (page - 1) * pageSize;

        _logger.LogInformation(
            "Fetching exam centers page {Page} (pageSize: {PageSize}, skip: {Skip}) — name: '{Name}', code: '{Code}', zone: '{Zone}', region: '{Region}', type: '{Type}', status: '{Status}'",
            page, pageSize, skip, name, code, zone, region, type, status);

        var filterConditions = new List<string>();

        if (!string.IsNullOrWhiteSpace(name))
            filterConditions.Add($"contains(Name, '{ODataEscape(name)}')");

        if (!string.IsNullOrWhiteSpace(code))
            filterConditions.Add($"Code eq '{ODataEscape(code)}'");

        if (!string.IsNullOrWhiteSpace(zone))
            filterConditions.Add($"Exam_Zone eq '{ODataEscape(zone)}'");

        if (!string.IsNullOrWhiteSpace(region))
            filterConditions.Add($"Exam_Region eq '{ODataEscape(region)}'");

        if (!string.IsNullOrWhiteSpace(type))
            filterConditions.Add($"Type eq '{ODataEscape(type)}'");

        if (!string.IsNullOrWhiteSpace(status))
            filterConditions.Add($"Status eq '{ODataEscape(status)}'");

        if (disabilityFriendly.HasValue)
            filterConditions.Add($"Disability_Friendly eq {(disabilityFriendly.Value ? "true" : "false")}");

        if (computerBasedFriendly.HasValue)
            filterConditions.Add($"Computer_Based_Friendly eq {(computerBasedFriendly.Value ? "true" : "false")}");

        var queryParams = new List<string>
        {
            $"$top={pageSize}",
            $"$skip={skip}"
        };

        if (filterConditions.Count > 0)
        {
            queryParams.Add($"$filter={string.Join(" and ", filterConditions)}");
        }

        var url = $"{_examCenterCardEntity}?{string.Join("&", queryParams)}";

        // Concurrently run fetcher with zone and region lookup loading
        var centersTask = _fetcher.FetchSinglePageAsync<ExamCenterCardRecord>(url, cancellationToken);
        var zonesTask = GetZoneLookupAsync(cancellationToken);
        var regionsTask = GetRegionLookupAsync(cancellationToken);

        await Task.WhenAll(centersTask, zonesTask, regionsTask);

        var records = centersTask.Result;
        var zoneLookup = zonesTask.Result;
        var regionLookup = regionsTask.Result;

        _logger.LogInformation("ExamCenterCard: {Count} records returned for page {Page}", records.Count, page);

        var items = records
            .Select(r => MapToDto(r, zoneLookup, regionLookup))
            .Where(dto => !isTrainingInstitution.HasValue || dto.IsTrainingInstitution == isTrainingInstitution.Value)
            .ToList();

        return new PagedResponse<ExamCenterDto>
        {
            Page = page,
            PageSize = pageSize,
            Count = items.Count,
            Data = items
        };
    }

    /// <summary>
    /// Returns a single exam center by its code (e.g. "1001"), enriched with zone and region details.
    /// </summary>
    public async Task<ExamCenterDto?> GetExamCenterByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching exam center by code: {Code}", code);

        var filter = $"Code eq '{ODataEscape(code)}'";
        var url = $"{_examCenterCardEntity}?$filter={filter}";

        var centersTask = _fetcher.FetchSinglePageAsync<ExamCenterCardRecord>(url, cancellationToken);
        var zonesTask = GetZoneLookupAsync(cancellationToken);
        var regionsTask = GetRegionLookupAsync(cancellationToken);

        await Task.WhenAll(centersTask, zonesTask, regionsTask);

        var records = centersTask.Result;
        var record = records.FirstOrDefault();

        if (record is null)
        {
            _logger.LogWarning("No ExamCenterCard record found for code: {Code}", code);
            return null;
        }

        return MapToDto(record, zonesTask.Result, regionsTask.Result);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Mapping & Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static ExamCenterDto MapToDto(
        ExamCenterCardRecord r,
        Dictionary<string, string> zoneLookup,
        Dictionary<string, string> regionLookup)
    {
        var zoneCode = (r.ExamZone ?? string.Empty).Trim();
        var regionCode = (r.ExamRegion ?? string.Empty).Trim();

        zoneLookup.TryGetValue(zoneCode, out var zoneName);
        regionLookup.TryGetValue(regionCode, out var regionName);

        var hasTrainingInst = !string.IsNullOrWhiteSpace(r.TrainingInstitution) || !string.IsNullOrWhiteSpace(r.Institution);

        return new ExamCenterDto
        {
            Code = r.Code ?? string.Empty,
            Name = r.Name ?? string.Empty,
            ExamZone = zoneCode,
            ZoneName = zoneName ?? string.Empty,
            ExamRegion = regionCode,
            RegionName = regionName ?? string.Empty,
            ExamRoute = r.ExamRoute ?? string.Empty,
            DisabilityFriendly = r.DisabilityFriendly,
            ComputerBasedFriendly = r.ComputerBasedFriendly,
            KisebCenters = r.KisebCenters,
            ExamBufferZone = r.ExamBufferZone,
            NeutralBufferZone = r.NeutralBufferZone,
            Blocked = r.Blocked,
            TrainingInstitution = r.TrainingInstitution ?? string.Empty,
            Institution = r.Institution ?? string.Empty,
            IsTrainingInstitution = hasTrainingInst,
            Type = r.Type ?? string.Empty,
            Status = r.Status ?? string.Empty,
            MaximumCapacityPerSession = r.MaximumCapacityPerSession ?? 0,
            TotalBookedStudents = r.TotalBookedStudents ?? 0,
            Address = r.Address ?? string.Empty,
            Address2 = r.Address2 ?? string.Empty,
            PostCode = r.PostCode ?? string.Empty,
            City = r.City ?? string.Empty,
            CountryRegionCode = r.CountryRegionCode ?? string.Empty,
            ShowMap = r.ShowMap ?? string.Empty,
            Contact = r.Contact ?? string.Empty,
            PhoneNo = r.PhoneNo ?? string.Empty,
            FaxNo = r.FaxNo ?? string.Empty,
            Email = r.Email ?? string.Empty,
            HomePage = r.HomePage ?? string.Empty
        };
    }

    private static string ODataEscape(string value) => value.Replace("'", "''");
}
