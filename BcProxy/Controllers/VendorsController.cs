using BcProxy.Models;
using BcProxy.Services;
using Microsoft.AspNetCore.Mvc;

namespace BcProxy.Controllers;

[ApiController]
[Route("[controller]")]
public class VendorsController : ControllerBase
{
    private readonly VendorService _vendorService;
    private readonly ILogger<VendorsController> _logger;

    public VendorsController(VendorService vendorService, ILogger<VendorsController> logger)
    {
        _vendorService = vendorService;
        _logger = logger;
    }

    /// <summary>
    /// Returns a paginated list of vendors from Business Central VendorCard with interpreted supplier categories.
    /// Optionally filter by name (contains), vendor code (exact), vendor type (exact), or supplier category (exact).
    /// </summary>
    /// <param name="page">1-based page number (default 1)</param>
    /// <param name="pageSize">Number of records per page (default 100, max 1000)</param>
    /// <param name="name">Optional partial name filter (case-insensitive contains)</param>
    /// <param name="no">Optional exact vendor code (No) filter (e.g. "00066")</param>
    /// <param name="vendorType">Optional vendor type filter (e.g. "Trade")</param>
    /// <param name="category">Optional supplier category code filter (e.g. "KAS 001")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet]
    public async Task<ActionResult<PagedResponse<VendorDto>>> GetAllVendors(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        [FromQuery] string? name = null,
        [FromQuery] string? no = null,
        [FromQuery] string? vendorType = null,
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("GET /vendors?page={Page}&pageSize={PageSize}&name={Name}&no={No}&vendorType={VendorType}&category={Category}",
                page, pageSize, name, no, vendorType, category);

            var result = await _vendorService.GetVendorsPagedAsync(
                page, pageSize, name, no, vendorType, category, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "fetching vendor list");
        }
    }

    /// <summary>
    /// Returns the list of all supplier categories and their descriptions.
    /// </summary>
    [HttpGet("categories")]
    public async Task<ActionResult<List<SupplierCategoryDto>>> GetCategories(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("GET /vendors/categories");
            var categories = await _vendorService.GetAllCategoriesAsync(cancellationToken);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "fetching supplier categories");
        }
    }

    /// <summary>
    /// Returns the details for a single vendor by their vendor code (No) with interpreted supplier category.
    /// </summary>
    /// <param name="no">Vendor code (No) (e.g. "00066")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpGet("{no}")]
    public async Task<ActionResult<VendorDto>> GetByNo(
        string no,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("GET /vendors/{No}", no);

            var result = await _vendorService.GetVendorByNoAsync(no, cancellationToken);

            return result is null
                ? NotFound(new { error = "Not Found", message = $"No vendor found with code '{no}'" })
                : Ok(result);
        }
        catch (Exception ex)
        {
            return HandleException(ex, $"fetching vendor {no}");
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
