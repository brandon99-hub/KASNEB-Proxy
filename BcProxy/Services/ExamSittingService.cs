using BcProxy.Models;
using Microsoft.Extensions.Caching.Memory;

namespace BcProxy.Services;

/// <summary>
/// Service for querying master Examination Sitting Cycles from Business Central ERP.
/// Provides a directory of all defined examination sittings, timelines, sequences, and closure flags.
/// </summary>
public class ExamSittingService
{
    private readonly ODataFetcher _fetcher;
    private readonly ILogger<ExamSittingService> _logger;
    private readonly IMemoryCache _cache;
    private readonly string _sittingCycleEntity;

    private const string SittingsCacheKey = "ExaminationSittingCycles_Cache";

    public ExamSittingService(
        ODataFetcher fetcher,
        ILogger<ExamSittingService> logger,
        IMemoryCache cache,
        IConfiguration configuration)
    {
        _fetcher = fetcher;
        _logger = logger;
        _cache = cache;
        _sittingCycleEntity = configuration["BusinessCentral:ExaminationSittingCycleEntity"]
            ?? "ExaminationSittingCycle";
    }

    /// <summary>
    /// Returns the full list of defined Examination Sitting Cycles, cached in memory for 1 hour.
    /// Supports optional filtering by closure status (closed) and sitting status (e.g. "Active").
    /// </summary>
    public async Task<List<ExaminationSittingCycleDto>> GetAllSittingsAsync(
        bool? closed = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        List<ExaminationSittingCycleDto> allSittings;

        if (_cache.TryGetValue(SittingsCacheKey, out List<ExaminationSittingCycleDto>? cached) && cached != null)
        {
            allSittings = cached;
        }
        else
        {
            try
            {
                _logger.LogInformation("Fetching examination sitting cycles from entity '{Entity}'", _sittingCycleEntity);
                var records = await _fetcher.FetchAllAsync<ExaminationSittingCycleRecord>(_sittingCycleEntity, cancellationToken);

                allSittings = records
                    .Where(r => !string.IsNullOrWhiteSpace(r.ExamSittingCycle))
                    .Select(MapToDto)
                    .ToList();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _cache.Set(SittingsCacheKey, allSittings, cacheOptions);
                _logger.LogInformation("Loaded and cached {Count} examination sitting cycles", allSittings.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch ExaminationSittingCycle from Business Central");
                return new List<ExaminationSittingCycleDto>();
            }
        }

        var query = allSittings.AsEnumerable();

        if (closed.HasValue)
        {
            query = query.Where(s => s.Closed == closed.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(s => string.Equals(s.SittingStatus, status, StringComparison.OrdinalIgnoreCase));
        }

        return query.ToList();
    }

    /// <summary>
    /// Returns details for a specific examination sitting cycle by name (e.g. "APRIL 2024").
    /// </summary>
    public async Task<ExaminationSittingCycleDto?> GetSittingByNameAsync(
        string cycleName,
        CancellationToken cancellationToken = default)
    {
        var allSittings = await GetAllSittingsAsync(cancellationToken: cancellationToken);

        return allSittings.FirstOrDefault(s =>
            string.Equals(s.ExamSittingCycle, cycleName.Trim(), StringComparison.OrdinalIgnoreCase) ||
            string.Equals(s.ExaminationProjectId, cycleName.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Mapping
    // ──────────────────────────────────────────────────────────────────────────

    private static ExaminationSittingCycleDto MapToDto(ExaminationSittingCycleRecord r) => new()
    {
        ExamSittingCycle = (r.ExamSittingCycle ?? string.Empty).Trim(),
        ExaminationProjectId = (r.ExaminationProjectId ?? string.Empty).Trim(),
        ProjectDescription = (r.ProjectDescription ?? string.Empty).Trim(),
        ExamStartDate = r.ExamStartDate ?? string.Empty,
        ExamEndDate = r.ExamEndDate ?? string.Empty,
        Closed = r.Closed,
        SittingStatus = (r.SittingStatus ?? string.Empty).Trim(),
        SittingSequence = r.SittingSequence ?? 0,
        DeferedSitting = (r.DeferedSitting ?? string.Empty).Trim()
    };
}
