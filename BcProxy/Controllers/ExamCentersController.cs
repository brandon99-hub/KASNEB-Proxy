using BcProxy.Models;
using BcProxy.Services;
using Microsoft.AspNetCore.Mvc;

namespace BcProxy.Controllers;

[ApiController]
[Route("[controller]")]
public class ExamCentersController : ControllerBase
{
    private readonly ExamCenterService _examCenterService;
    private readonly ILogger<ExamCentersController> _logger;

    public ExamCentersController(
        ExamCenterService examCenterService,
        ILogger<ExamCentersController> logger)
    {
        _examCenterService = examCenterService;
        _logger = logger;
    }

    /// <summary>
    /// Returns a paginated list of Exam Centers from Business Central ERP.
    /// Enriched with resolved human-readable Zone and Region descriptions.
    /// Supports filtering by center name, center code, zone code, region code, center type,
    /// status, and whether the venue serves as a training institution.
    /// </summary>
    /// <param name="page">1-based page number (default 1)</param>
    /// <param name="pageSize">Number of records per page (default 100, max 1000)</param>
    /// <param name="name">Optional partial center name filter</param>
    /// <param name="code">Optional exact center code filter (e.g. "1001")</param>
    /// <param name="zone">Optional zone code filter (e.g. "6")</param>
    /// <param name="region">Optional region code filter (e.g. "49")</param>
    /// <param name="type">Optional center type filter (e.g. "Foreign", "Local")</param>
    /// <param name="status">Optional operational status filter (e.g. "Active")</param>
    /// <param name="isTrainingInstitution">Optional boolean filter to retrieve centers that also function as training institutions</param>
    /// <param name="disabilityFriendly">Optional boolean filter for disability accessible centers</param>
    /// <param name="computerBasedFriendly">Optional boolean filter for computer-based testing centers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<ExamCenterDto>>> GetAllExamCenters(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        [FromQuery] string? name = null,
        [FromQuery] string? code = null,
        [FromQuery] string? zone = null,
        [FromQuery] string? region = null,
        [FromQuery] string? type = null,
        [FromQuery] string? status = null,
        [FromQuery] bool? isTrainingInstitution = null,
        [FromQuery] bool? disabilityFriendly = null,
        [FromQuery] bool? computerBasedFriendly = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "GET /examcenters?page={Page}&pageSize={PageSize}&name={Name}&code={Code}&zone={Zone}&region={Region}&type={Type}&status={Status}&isTrainingInst={IsTraining}",
                page, pageSize, name, code, zone, region, type, status, isTrainingInstitution);

            var result = await _examCenterService.GetExamCentersPagedAsync(
                page, pageSize, name, code, zone, region, type, status,
                isTrainingInstitution, disabilityFriendly, computerBasedFriendly, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "fetching exam centers list");
        }
    }

    /// <summary>
    /// Returns the complete list of Exam Zones with zone names and center counts.
    /// </summary>
    [HttpGet("zones")]
    public async Task<ActionResult<List<ExamZoneDto>>> GetZones(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("GET /examcenters/zones");
            var zones = await _examCenterService.GetAllZonesAsync(cancellationToken);
            return Ok(zones);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "fetching exam zones");
        }
    }

    /// <summary>
    /// Returns the complete list of Exam Regions with descriptions and zone/center statistics.
    /// </summary>
    [HttpGet("regions")]
    public async Task<ActionResult<List<ExamRegionDto>>> GetRegions(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("GET /examcenters/regions");
            var regions = await _examCenterService.GetAllRegionsAsync(cancellationToken);
            return Ok(regions);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "fetching exam regions");
        }
    }

    /// <summary>
    /// Returns details for a single exam center by its code (e.g. "1001"), enriched with zone and region details.
    /// </summary>
    /// <param name="code">Exam center code (e.g. "1001")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("{code}")]
    public async Task<ActionResult<ExamCenterDto>> GetByCode(
        string code,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GET /examcenters/{Code}", code);

            var result = await _examCenterService.GetExamCenterByCodeAsync(code, cancellationToken);

            return result is null
                ? NotFound(new { error = "Not Found", message = $"No exam center found with code '{code}'" })
                : Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"fetching exam center {code}");
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Shared error handler
    // ──────────────────────────────────────────────────────────────────────────

    private ObjectResult HandleException(Exception ex, string context)
    {
        switch (ex)
        {
            case TaskCanceledException:
                _logger.LogError("Timeout {Context}", context);
                return StatusCode(504, new { error = "Gateway Timeout", message = "Request to Business Central timed out" });

            case BusinessCentralException bcEx:
                _logger.LogError(bcEx, "BC error {Context}", context);
                return bcEx.StatusCode.HasValue
                    ? StatusCode((int)bcEx.StatusCode.Value, new { error = "Business Central Error", message = bcEx.Message })
                    : StatusCode(502, new { error = "Bad Gateway", message = "Failed to reach Business Central" });

            case HttpRequestException httpEx:
                _logger.LogError(httpEx, "HTTP error {Context}", context);
                return StatusCode(502, new { error = "Bad Gateway", message = "Failed to reach Business Central" });

            default:
                _logger.LogError(ex, "Unexpected error {Context}", context);
                return StatusCode(500, new { error = "Internal Server Error", message = "An unexpected error occurred" });
        }
    }
}
