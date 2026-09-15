using BcProxy.Models;

namespace BcProxy.Services;

/// <summary>
/// Service for querying Student Registration applications/records from Business Central ERP.
/// Operates independently from the enrolled student profile service.
/// </summary>
public class StudentRegistrationService
{
    private readonly ODataFetcher _fetcher;
    private readonly ILogger<StudentRegistrationService> _logger;
    private readonly string _studentRegistrationsEntity;

    public StudentRegistrationService(
        ODataFetcher fetcher,
        ILogger<StudentRegistrationService> logger,
        IConfiguration configuration)
    {
        _fetcher = fetcher;
        _logger = logger;
        _studentRegistrationsEntity = configuration["BusinessCentral:StudentRegistrationsEntity"]
            ?? "Studentregistrations";
    }

    /// <summary>
    /// Returns a paginated list of student registrations from Business Central.
    /// Supports filtering by approval status (e.g. "Open", "Rejected"), examination ID,
    /// National ID / Passport, student customer number, and student name.
    /// </summary>
    public async Task<PagedResponse<StudentRegistrationDto>> GetRegistrationsPagedAsync(
        int page = 1,
        int pageSize = 100,
        string? approvalStatus = null,
        string? examinationId = null,
        string? idNumber = null,
        string? studentNo = null,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 50;
        if (pageSize > 1000) pageSize = 1000;

        int skip = (page - 1) * pageSize;

        _logger.LogInformation(
            "Fetching student registrations page {Page} (pageSize: {PageSize}, skip: {Skip}) — status: '{Status}', exam: '{Exam}', idNo: '{IdNo}', studentNo: '{StudentNo}', name: '{Name}'",
            page, pageSize, skip, approvalStatus, examinationId, idNumber, studentNo, name);

        var filterConditions = new List<string>();

        if (!string.IsNullOrWhiteSpace(approvalStatus))
            filterConditions.Add($"Approval_Status eq '{ODataEscape(approvalStatus)}'");

        if (!string.IsNullOrWhiteSpace(examinationId))
            filterConditions.Add($"Examination_ID eq '{ODataEscape(examinationId)}'");

        if (!string.IsNullOrWhiteSpace(idNumber))
            filterConditions.Add($"ID_Number_Passport_No eq '{ODataEscape(idNumber)}'");

        if (!string.IsNullOrWhiteSpace(studentNo))
            filterConditions.Add($"Student_No eq '{ODataEscape(studentNo)}'");

        if (!string.IsNullOrWhiteSpace(name))
            filterConditions.Add($"contains(Student_Name, '{ODataEscape(name)}')");

        var queryParams = new List<string>
        {
            $"$top={pageSize}",
            $"$skip={skip}"
        };

        if (filterConditions.Count > 0)
        {
            queryParams.Add($"$filter={string.Join(" and ", filterConditions)}");
        }

        var url = $"{_studentRegistrationsEntity}?{string.Join("&", queryParams)}";
        var records = await _fetcher.FetchSinglePageAsync<StudentRegistrationRecord>(url, cancellationToken);

        _logger.LogInformation("Studentregistrations: {Count} records returned for page {Page}", records.Count, page);

        var items = records.Select(MapToDto).ToList();

        return new PagedResponse<StudentRegistrationDto>
        {
            Page = page,
            PageSize = pageSize,
            Count = items.Count,
            Data = items
        };
    }

    /// <summary>
    /// Returns a single student registration by its unique registration document / voucher No.
    /// </summary>
    public async Task<StudentRegistrationDto?> GetRegistrationByNoAsync(
        string no,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching student registration by No: {No}", no);

        var filter = $"No eq '{ODataEscape(no)}'";
        var url = $"{_studentRegistrationsEntity}?$filter={filter}";

        var records = await _fetcher.FetchSinglePageAsync<StudentRegistrationRecord>(url, cancellationToken);
        var record = records.FirstOrDefault();

        if (record is null)
        {
            _logger.LogWarning("No Studentregistrations record found for No: {No}", no);
            return null;
        }

        return MapToDto(record);
    }

    /// <summary>
    /// Returns all student registrations associated with a specific Student Customer No (e.g. "ST00545448").
    /// </summary>
    public async Task<List<StudentRegistrationDto>> GetRegistrationsByStudentNoAsync(
        string studentNo,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching student registrations for Student_No: {StudentNo}", studentNo);

        var filter = $"Student_No eq '{ODataEscape(studentNo)}'";
        var url = $"{_studentRegistrationsEntity}?$filter={filter}";

        var records = await _fetcher.FetchAllAsync<StudentRegistrationRecord>(url, cancellationToken);
        return records.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Returns all student registrations associated with a specific National ID or Passport No.
    /// </summary>
    public async Task<List<StudentRegistrationDto>> GetRegistrationsByIdNumberAsync(
        string idNumber,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching student registrations for ID_Number_Passport_No: {IdNumber}", idNumber);

        var filter = $"ID_Number_Passport_No eq '{ODataEscape(idNumber)}'";
        var url = $"{_studentRegistrationsEntity}?$filter={filter}";

        var records = await _fetcher.FetchAllAsync<StudentRegistrationRecord>(url, cancellationToken);
        return records.Select(MapToDto).ToList();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Mapping & Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static StudentRegistrationDto MapToDto(StudentRegistrationRecord r) => new()
    {
        No = r.No ?? string.Empty,
        StudentNo = r.StudentNo ?? string.Empty,
        StudentName = r.StudentName ?? string.Empty,
        IdNumberPassportNo = r.IdNumberPassportNo ?? string.Empty,
        ExaminationId = r.ExaminationId ?? string.Empty,
        ExaminationDescription = r.ExaminationDescription ?? string.Empty,
        Gender = r.Gender ?? string.Empty,
        DateOfBirth = r.DateOfBirth ?? string.Empty,
        Disabled = r.Disabled,
        NcpwdNo = r.NcpwdNo ?? string.Empty,
        RegistrationDate = r.RegistrationDate ?? string.Empty,
        CreatedOn = r.CreatedOn ?? string.Empty,
        EmailSent = r.EmailSent,
        ApprovalStatus = r.ApprovalStatus ?? string.Empty,
        Email = r.Email ?? string.Empty,
        HighestAcademicQualification = r.HighestAcademicQualification ?? string.Empty,
        HighestAcademicQCode = r.HighestAcademicQCode ?? string.Empty
    };

    private static string ODataEscape(string value) => value.Replace("'", "''");
}
