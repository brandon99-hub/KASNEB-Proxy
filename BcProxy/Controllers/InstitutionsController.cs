using BcProxy.Models;
using BcProxy.Services;
using Microsoft.AspNetCore.Mvc;

namespace BcProxy.Controllers;

[ApiController]
[Route("[controller]")]
public class InstitutionsController : ControllerBase
{
    private readonly InstitutionService _institutionService;
    private readonly ILogger<InstitutionsController> _logger;

    public InstitutionsController(InstitutionService institutionService, ILogger<InstitutionsController> logger)
    {
        _institutionService = institutionService;
        _logger = logger;
    }

    /// <summary>
    /// Returns a paginated list of training institutions from Business Central.
    /// Optionally filter by name (contains), institution code (exact), or accreditation status (exact).
    /// </summary>
    /// <param name="page">1-based page number (default 1)</param>
    /// <param name="pageSize">Number of records per page (default 100, max 1000)</param>
    /// <param name="name">Optional partial name filter (case-insensitive contains)</param>
    /// <param name="code">Optional exact institution code (No) filter (e.g. "CUST00004")</param>
    /// <param name="status">Optional accreditation status filter (e.g. "FULL")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<InstitutionDto>>> GetAllInstitutions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        [FromQuery] string? name = null,
        [FromQuery] string? code = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("GET /institutions?page={Page}&pageSize={PageSize}&name={Name}&code={Code}&status={Status}",
                page, pageSize, name, code, status);

            var result = await _institutionService.GetInstitutionsPagedAsync(
                page, pageSize, name, code, status, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "fetching training institutions list");
        }
    }

    /// <summary>
    /// Returns the details for a single training institution by its institution code (No).
    /// </summary>
    /// <param name="code">Institution code (No) (e.g. "CUST00004" or "CUST00013")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("{code}")]
    public async Task<ActionResult<InstitutionDto>> GetByCode(
        string code,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GET /institutions/{Code}", code);

            var result = await _institutionService.GetInstitutionByCodeAsync(code, cancellationToken);

            return result is null
                ? NotFound(new { error = "Not Found", message = $"No training institution found with code '{code}'" })
                : Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"fetching training institution {code}");
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
