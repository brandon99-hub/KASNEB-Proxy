using BcProxy.Models;
using BcProxy.Services;
using Microsoft.AspNetCore.Mvc;

namespace BcProxy.Controllers;

[ApiController]
[Route("[controller]")]
public class StudentRegistrationsController : ControllerBase
{
    private readonly StudentRegistrationService _registrationService;
    private readonly ILogger<StudentRegistrationsController> _logger;

    public StudentRegistrationsController(
        StudentRegistrationService registrationService,
        ILogger<StudentRegistrationsController> logger)
    {
        _registrationService = registrationService;
        _logger = logger;
    }

    /// <summary>
    /// Returns a paginated list of new student registrations from Business Central ERP.
    /// Supports filtering by approval status (e.g. "Open", "Rejected"), examination ID,
    /// National ID / Passport, student number, and student name.
    /// </summary>
    /// <param name="page">1-based page number (default 1)</param>
    /// <param name="pageSize">Number of records per page (default 100, max 1000)</param>
    /// <param name="status">Optional approval status filter (e.g. "Open" or "Rejected")</param>
    /// <param name="examinationId">Optional examination qualification filter (e.g. "CPA")</param>
    /// <param name="idNumber">Optional exact National ID / Passport filter</param>
    /// <param name="studentNo">Optional exact Student No filter (e.g. "ST00545448")</param>
    /// <param name="name">Optional partial student name filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<StudentRegistrationDto>>> GetAllRegistrations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        [FromQuery] string? status = null,
        [FromQuery] string? examinationId = null,
        [FromQuery] string? idNumber = null,
        [FromQuery] string? studentNo = null,
        [FromQuery] string? name = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "GET /studentregistrations?page={Page}&pageSize={PageSize}&status={Status}&examinationId={Exam}&idNumber={Id}&studentNo={StudentNo}&name={Name}",
                page, pageSize, status, examinationId, idNumber, studentNo, name);

            var result = await _registrationService.GetRegistrationsPagedAsync(
                page, pageSize, status, examinationId, idNumber, studentNo, name, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "fetching student registrations list");
        }
    }

    /// <summary>
    /// Returns a single student registration record by its registration document / voucher No.
    /// </summary>
    /// <param name="no">Registration document / voucher No (e.g. "10023" or "AP0001")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("{no}")]
    public async Task<ActionResult<StudentRegistrationDto>> GetByNo(
        string no,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GET /studentregistrations/{No}", no);

            var result = await _registrationService.GetRegistrationByNoAsync(no, cancellationToken);

            return result is null
                ? NotFound(new { error = "Not Found", message = $"No student registration found with No '{no}'" })
                : Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"fetching student registration {no}");
        }
    }

    /// <summary>
    /// Returns all registration records for a specific Student Customer No (e.g. "ST00545448").
    /// </summary>
    /// <param name="studentNo">Student Customer No (e.g. "ST00545448")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("by-student/{studentNo}")]
    public async Task<ActionResult<List<StudentRegistrationDto>>> GetByStudentNo(
        string studentNo,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GET /studentregistrations/by-student/{StudentNo}", studentNo);

            var result = await _registrationService.GetRegistrationsByStudentNoAsync(studentNo, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"fetching student registrations for student {studentNo}");
        }
    }

    /// <summary>
    /// Returns all registration records for a specific National ID or Passport Number.
    /// </summary>
    /// <param name="idNumber">National ID / Passport number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("by-id/{idNumber}")]
    public async Task<ActionResult<List<StudentRegistrationDto>>> GetByIdNumber(
        string idNumber,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GET /studentregistrations/by-id/{IdNumber}", idNumber);

            var result = await _registrationService.GetRegistrationsByIdNumberAsync(idNumber, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"fetching student registrations for ID {idNumber}");
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
