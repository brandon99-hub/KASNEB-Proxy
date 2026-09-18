using BcProxy.Models;
using BcProxy.Services;
using Microsoft.AspNetCore.Mvc;

namespace BcProxy.Controllers;

[ApiController]
[Route("[controller]")]
public class SittingsController : ControllerBase
{
    private readonly ExamSittingService _sittingService;
    private readonly ILogger<SittingsController> _logger;

    public SittingsController(
        ExamSittingService sittingService,
        ILogger<SittingsController> logger)
    {
        _sittingService = sittingService;
        _logger = logger;
    }

    /// <summary>
    /// Returns the complete directory of defined Examination Sitting Cycles from Business Central ERP.
    /// Provides exam start/end dates, sequence ordering, closure flags, and deferred target sittings.
    /// </summary>
    /// <param name="closed">Optional filter by closure status (e.g. true for closed sittings, false for upcoming/open)</param>
    /// <param name="status">Optional filter by sitting status (e.g. "Active")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    public async Task<ActionResult<List<ExaminationSittingCycleDto>>> GetAllSittings(
        [FromQuery] bool? closed = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("GET /sittings?closed={Closed}&status={Status}", closed, status);

            var result = await _sittingService.GetAllSittingsAsync(closed, status, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "fetching examination sitting cycles");
        }
    }

    /// <summary>
    /// Returns details for a specific examination sitting cycle by name (e.g. "APRIL 2024" or "DECEMBER 2022").
    /// </summary>
    /// <param name="cycleName">Exam sitting cycle name or project ID (e.g. "APRIL 2024")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("{cycleName}")]
    public async Task<ActionResult<ExaminationSittingCycleDto>> GetByCycleName(
        string cycleName,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GET /sittings/{CycleName}", cycleName);

            var result = await _sittingService.GetSittingByNameAsync(cycleName, cancellationToken);

            return result is null
                ? NotFound(new { error = "Not Found", message = $"No examination sitting found matching '{cycleName}'" })
                : Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"fetching examination sitting {cycleName}");
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
