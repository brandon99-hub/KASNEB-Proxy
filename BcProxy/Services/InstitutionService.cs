using BcProxy.Models;

namespace BcProxy.Services;

/// <summary>
/// Service for querying Training Institution data from Business Central.
/// </summary>
public class InstitutionService
{
    private readonly ODataFetcher _fetcher;
    private readonly ILogger<InstitutionService> _logger;
    private readonly string _trainingInstitutionsEntity;

    public InstitutionService(
        ODataFetcher fetcher,
        ILogger<InstitutionService> logger,
        IConfiguration configuration)
    {
        _fetcher = fetcher;
        _logger = logger;
        _trainingInstitutionsEntity = configuration["BusinessCentral:TrainingInstitutionsEntity"]
            ?? "Training_Institutions";
    }

    /// <summary>
    /// Returns a paginated list of training institutions from Business Central.
    /// Supports optional filtering by name (contains), institution code (exact), or accreditation status (exact).
    /// </summary>
    public async Task<PagedResponse<InstitutionDto>> GetInstitutionsPagedAsync(
        int page = 1,
        int pageSize = 100,
        string? name = null,
        string? code = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 50;
        if (pageSize > 1000) pageSize = 1000;

        int skip = (page - 1) * pageSize;

        _logger.LogInformation("Fetching institutions page {Page} (pageSize: {PageSize}, skip: {Skip}) — name: '{Name}', code: '{Code}', status: '{Status}'",
            page, pageSize, skip, name, code, status);

        var filterConditions = new List<string>();

        if (!string.IsNullOrWhiteSpace(name))
            filterConditions.Add($"contains(Name, '{ODataEscape(name)}')");

        if (!string.IsNullOrWhiteSpace(code))
            filterConditions.Add($"No eq '{ODataEscape(code)}'");

        if (!string.IsNullOrWhiteSpace(status))
            filterConditions.Add($"Accreditation_Status eq '{ODataEscape(status)}'");

        var queryParams = new List<string>
        {
            $"$top={pageSize}",
            $"$skip={skip}"
        };

        if (filterConditions.Count > 0)
        {
            queryParams.Add($"$filter={string.Join(" and ", filterConditions)}");
        }

        var url = $"{_trainingInstitutionsEntity}?{string.Join("&", queryParams)}";
        var records = await _fetcher.FetchSinglePageAsync<TrainingInstitutionRecord>(url, cancellationToken);

        _logger.LogInformation("Training_Institutions: {Count} records returned for page {Page}", records.Count, page);

        var items = records.Select(MapToDto).ToList();

        return new PagedResponse<InstitutionDto>
        {
            Page = page,
            PageSize = pageSize,
            Count = items.Count,
            Data = items
        };
    }

    /// <summary>
    /// Returns a single institution by its code / No (e.g. "CUST00004").
    /// </summary>
    public async Task<InstitutionDto?> GetInstitutionByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching institution by code: {Code}", code);

        var filter = $"No eq '{ODataEscape(code)}'";
        var url = $"{_trainingInstitutionsEntity}?$filter={filter}";

        var records = await _fetcher.FetchSinglePageAsync<TrainingInstitutionRecord>(url, cancellationToken);
        var record = records.FirstOrDefault();

        if (record is null)
        {
            _logger.LogWarning("No Training_Institutions record found for code: {Code}", code);
            return null;
        }

        return MapToDto(record);
    }

    /// <summary>
    /// Fetches all institutions from Business Central by following @odata.nextLink continuation tokens.
    /// </summary>
    public async Task<List<InstitutionDto>> GetAllInstitutionsAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all institutions across all pages");

        var records = await _fetcher.FetchAllAsync<TrainingInstitutionRecord>(_trainingInstitutionsEntity, cancellationToken);
        return records.Select(MapToDto).ToList();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Mapping & Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static InstitutionDto MapToDto(TrainingInstitutionRecord r) => new()
    {
        No = r.No ?? string.Empty,
        Name = r.Name ?? string.Empty,
        Address = r.Address ?? string.Empty,
        Address2 = r.Address2 ?? string.Empty,
        City = r.City ?? string.Empty,
        Contact = r.Contact ?? string.Empty,
        PhoneNo = r.PhoneNo ?? string.Empty,
        Gender = r.Gender ?? string.Empty,
        AccreditationStatus = r.AccreditationStatus ?? string.Empty,
        DateOfBirth = r.DateOfBirth ?? string.Empty,
        Disabled = r.Disabled,
        AccreditationStartDate = r.AccreditationStartDate ?? string.Empty,
        AccreditationEndDate = r.AccreditationEndDate ?? string.Empty,
        NcpwdNo = r.NcpwdNo ?? string.Empty
    };

    private static string ODataEscape(string value) => value.Replace("'", "''");
}
