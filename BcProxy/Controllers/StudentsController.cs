using BcProxy.Models;
using BcProxy.Services;
using Microsoft.AspNetCore.Mvc;

namespace BcProxy.Controllers;

[ApiController]
[Route("[controller]")]
public class StudentsController : ControllerBase
{
    private readonly StudentProfileService _profileService;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(StudentProfileService profileService, ILogger<StudentsController> logger)
    {
        _profileService = profileService;
        _logger = logger;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // LIST endpoint — bio-data only (CRM student list page)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a paginated page of students with bio-data only (no exam accounts or ledger entries).
    /// Designed for fast, sub-second populating of CRM list/table views.
    /// Optionally filter by name (contains) or ID number (exact match).
    /// </summary>
    /// <param name="page">1-based page number (default 1)</param>
    /// <param name="pageSize">Number of records per page (default 100, max 1000)</param>
    /// <param name="name">Optional partial name filter (case-insensitive contains)</param>
    /// <param name="idNo">Optional exact National ID number filter</param>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<StudentSummary>>> GetAllStudents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        [FromQuery] string? name = null,
        [FromQuery] string? idNo = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("GET /students?page={Page}&pageSize={PageSize}&name={Name}&idNo={IdNo}",
                page, pageSize, name, idNo);

            var result = await _profileService.GetStudentsPagedAsync(page, pageSize, name, idNo, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "fetching student list");
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // DETAIL endpoints — full profile (bio + exam accounts + ledger)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the full profile for a student by their BC Customer No (e.g. ST00145181).
    /// Includes bio-data, all KASNEB exam accounts, and all ledger entries.
    /// </summary>
    /// <param name="customerNo">BC Customer No (e.g. "ST00145181")</param>
    [HttpGet("{customerNo}")]
    public async Task<ActionResult<StudentProfile>> GetByCustomerNo(
        string customerNo,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GET /students/{CustomerNo}", customerNo);

            var result = await _profileService.GetStudentByCustomerNoAsync(customerNo, cancellationToken);

            return result is null
                ? NotFound(new { error = "Not Found", message = $"No student found with Customer No '{customerNo}'" })
                : Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"fetching student {customerNo}");
        }
    }

    /// <summary>
    /// Returns the full profile for a student identified by their National ID number.
    /// </summary>
    /// <param name="idNo">National ID number (e.g. "36478037")</param>
    [HttpGet("by-id/{idNo}")]
    public async Task<ActionResult<StudentProfile>> GetByIdNo(
        string idNo,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GET /students/by-id/{IdNo}", idNo);

            var result = await _profileService.GetStudentByIdNoAsync(idNo, cancellationToken);

            return result is null
                ? NotFound(new { error = "Not Found", message = $"No student found with ID No '{idNo}'" })
                : Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"fetching student by ID {idNo}");
        }
    }

    /// <summary>
    /// Returns the full profile for a student identified by their KASNEB Registration No.
    /// </summary>
    /// <param name="registrationNo">KASNEB Registration No (e.g. "2021/CPA/03987" or "NAC/181912")</param>
    [HttpGet("by-registration/{registrationNo}")]
    public async Task<ActionResult<StudentProfile>> GetByRegistrationNo(
        string registrationNo,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GET /students/by-registration/{RegistrationNo}", registrationNo);

            var result = await _profileService.GetStudentByRegistrationNoAsync(registrationNo, cancellationToken);

            return result is null
                ? NotFound(new { error = "Not Found", message = $"No student found with Registration No '{registrationNo}'" })
                : Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"fetching student by registration {registrationNo}");
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
