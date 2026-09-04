using BcProxy.Models;
using System.Net;

namespace BcProxy.Services;

/// <summary>
/// Core service for KASNEB student stakeholder data.
///
/// Orchestrates eight BC OData web services:
///   1. Studentlist           — bio-data (No, ID_No, Phone_No, E_Mail, Gender, Balance)
///   2. ExamAccounts          — KASNEB registration accounts (Registration_No, Course, Status, Renewal)
///   3. customerEntries       — posted ledger entries (payments, invoices, credit memos)
///   4. ExemptionEntries      — granted paper exemptions (keyed by Stud_Reg_No)
///   5. PostedDeferment       — examination sitting deferral requests (keyed by Student_Reg_No)
///   6. StudentsExamBookings  — exam bookings / applications (keyed by Student_Reg_No)
///   7. ProcessedBookings     — confirmed bookings and allocated exam centers (keyed by Student_Reg_No)
///   8. ExamResults           — historical examination grades, marks, and sittings (keyed by Student_Reg_No)
/// </summary>
public class StudentProfileService
{
    private readonly ODataFetcher _fetcher;
    private readonly ILogger<StudentProfileService> _logger;
    private readonly string _studentListEntity;
    private readonly string _examAccountsEntity;
    private readonly string _customerEntriesEntity;
    private readonly string _exemptionEntriesEntity;
    private readonly string _postedDefermentEntity;
    private readonly string _studentsExamBookingsEntity;
    private readonly string _processedBookingsEntity;
    private readonly string _examResultsEntity;

    public StudentProfileService(
        ODataFetcher fetcher,
        ILogger<StudentProfileService> logger,
        IConfiguration configuration)
    {
        _fetcher = fetcher;
        _logger = logger;

        _studentListEntity = configuration["BusinessCentral:StudentListEntity"] ?? "Studentlist";
        _examAccountsEntity = configuration["BusinessCentral:ExamAccountsEntity"] ?? "ExamAccounts";
        _customerEntriesEntity = configuration["BusinessCentral:CustomerEntriesEntity"] ?? "customerEntries";
        _exemptionEntriesEntity = configuration["BusinessCentral:ExemptionEntriesEntity"] ?? "ExemptionEntries";
        _postedDefermentEntity = configuration["BusinessCentral:PostedDefermentEntity"] ?? "PostedDeferment";
        _studentsExamBookingsEntity = configuration["BusinessCentral:StudentsExamBookingsEntity"] ?? "StudentsExamBookings";
        _processedBookingsEntity = configuration["BusinessCentral:ProcessedBookingsEntity"] ?? "ProcessedBookings";
        _examResultsEntity = configuration["BusinessCentral:ExamResultsEntity"] ?? "ExamResults";
    }

    // ──────────────────────────────────────────────────────────────────────────
    // LIST — bio-data only (for CRM student list page with pagination)
    // ──────────────────────────────────────────────────────────────────────────

    public async Task<PagedResponse<StudentSummary>> GetStudentsPagedAsync(
        int page = 1,
        int pageSize = 100,
        string? nameFilter = null,
        string? idNoFilter = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 50;
        if (pageSize > 1000) pageSize = 1000;

        int skip = (page - 1) * pageSize;

        _logger.LogInformation("Fetching students page {Page} (pageSize: {PageSize}, skip: {Skip}) — name: '{Name}', idNo: '{IdNo}'",
            page, pageSize, skip, nameFilter, idNoFilter);

        var filterConditions = new List<string>();

        if (!string.IsNullOrWhiteSpace(nameFilter))
            filterConditions.Add($"contains(Name, '{ODataEscape(nameFilter)}')");

        if (!string.IsNullOrWhiteSpace(idNoFilter))
            filterConditions.Add($"ID_No eq '{ODataEscape(idNoFilter)}'");

        var queryParams = new List<string>
        {
            $"$top={pageSize}",
            $"$skip={skip}"
        };

        if (filterConditions.Count > 0)
        {
            queryParams.Add($"$filter={string.Join(" and ", filterConditions)}");
        }

        var url = $"{_studentListEntity}?{string.Join("&", queryParams)}";
        var records = await _fetcher.FetchSinglePageAsync<StudentListRecord>(url, cancellationToken);

        _logger.LogInformation("StudentList: {Count} records returned for page {Page}", records.Count, page);

        var items = records.Select(r => new StudentSummary
        {
            CustomerNo  = r.No      ?? string.Empty,
            Name        = r.Name    ?? string.Empty,
            IdNo        = r.IdNo    ?? string.Empty,
            PhoneNo     = r.PhoneNo ?? string.Empty,
            Email       = r.Email   ?? string.Empty,
            Gender      = r.Gender  ?? string.Empty,
            Address     = r.Address ?? string.Empty,
            City        = r.City    ?? string.Empty,
            Disabled    = r.Disabled,
            Balance     = r.Balance,
            BalanceLcy  = r.BalanceLcy,
            SalesLcy    = r.SalesLcy
        })
        .ToList();

        return new PagedResponse<StudentSummary>
        {
            Page = page,
            PageSize = pageSize,
            Count = items.Count,
            Data = items
        };
    }

    // ──────────────────────────────────────────────────────────────────────────
    // DETAIL — full 360-degree profile for a single student
    // ──────────────────────────────────────────────────────────────────────────

    public async Task<StudentProfile?> GetStudentByCustomerNoAsync(
        string customerNo,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching full 360 profile for Customer No: {CustomerNo}", customerNo);

        var bioList = await _fetcher.FetchAllAsync<StudentListRecord>(
            BuildUrl(_studentListEntity, [$"No eq '{ODataEscape(customerNo)}'"]), cancellationToken);

        var student = bioList.FirstOrDefault();
        if (student is null)
        {
            _logger.LogWarning("No Studentlist record found for Customer No: {CustomerNo}", customerNo);
            return null;
        }

        return await FetchAndBuildCompleteProfileAsync(student, customerNo, null, cancellationToken);
    }

    public async Task<StudentProfile?> GetStudentByIdNoAsync(
        string idNo,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching full profile for ID No: {IdNo}", idNo);

        var studentListUrl = BuildUrl(_studentListEntity, [$"ID_No eq '{ODataEscape(idNo)}'"]);
        var bioList = await _fetcher.FetchAllAsync<StudentListRecord>(studentListUrl, cancellationToken);
        var student = bioList.FirstOrDefault();

        if (student is null || string.IsNullOrEmpty(student.No))
        {
            _logger.LogWarning("No Studentlist record found for ID No: {IdNo}", idNo);
            return null;
        }

        return await FetchAndBuildCompleteProfileAsync(student, student.No, null, cancellationToken);
    }

    public async Task<StudentProfile?> GetStudentByRegistrationNoAsync(
        string registrationNo,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching full profile for Registration No: {RegistrationNo}", registrationNo);

        var examUrl = BuildUrl(_examAccountsEntity, [$"Registration_No eq '{ODataEscape(registrationNo)}'"]);
        var examList = await _fetcher.FetchAllAsync<ExamAccount>(examUrl, cancellationToken);

        var primaryExam = examList.FirstOrDefault();
        if (primaryExam is null || string.IsNullOrEmpty(primaryExam.StudentCustNo))
        {
            _logger.LogWarning("No ExamAccount found for Registration No: {RegistrationNo}", registrationNo);
            return null;
        }

        var customerNo = primaryExam.StudentCustNo;
        var bioList = await _fetcher.FetchAllAsync<StudentListRecord>(
            BuildUrl(_studentListEntity, [$"No eq '{ODataEscape(customerNo)}'"]), cancellationToken);

        var student = bioList.FirstOrDefault();
        if (student is null)
        {
            _logger.LogWarning("No Studentlist bio-data found for Customer No: {CustomerNo}", customerNo);
            return null;
        }

        return await FetchAndBuildCompleteProfileAsync(student, customerNo, registrationNo, cancellationToken);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Sub-resource helpers (for modular / tabbed CRM endpoints)
    // ──────────────────────────────────────────────────────────────────────────

    public async Task<List<string>> GetStudentRegistrationNosAsync(string customerNo, CancellationToken cancellationToken)
    {
        var examAccounts = await SafeFetchAsync<ExamAccount>(
            BuildUrl(_examAccountsEntity, [$"Student_Cust_No eq '{ODataEscape(customerNo)}'"]), cancellationToken);

        return examAccounts
            .Select(e => e.RegistrationNo)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<List<ExemptionDto>> GetExemptionsAsync(
        string customerNo,
        string? registrationNo = null,
        CancellationToken cancellationToken = default)
    {
        List<string> regNos = !string.IsNullOrWhiteSpace(registrationNo)
            ? new List<string> { registrationNo }
            : await GetStudentRegistrationNosAsync(customerNo, cancellationToken);

        string regFilter = regNos.Count > 0
            ? string.Join(" or ", regNos.Select(r => $"Stud_Reg_No eq '{ODataEscape(r)}'"))
            : $"Stud_Cust_No eq '{ODataEscape(customerNo)}'";

        var records = await SafeFetchAsync<ExemptionEntryRecord>(
            BuildUrl(_exemptionEntriesEntity, [$"({regFilter}) and Remove eq false"]),
            cancellationToken);

        return records.Select(MapToDto).ToList();
    }

    public async Task<List<DefermentDto>> GetDefermentsAsync(
        string customerNo,
        string? registrationNo = null,
        CancellationToken cancellationToken = default)
    {
        List<string> regNos = !string.IsNullOrWhiteSpace(registrationNo)
            ? new List<string> { registrationNo }
            : await GetStudentRegistrationNosAsync(customerNo, cancellationToken);

        string filter = regNos.Count > 0
            ? string.Join(" or ", regNos.Select(r => $"Student_Reg_No eq '{ODataEscape(r)}'"))
            : $"Student_No eq '{ODataEscape(customerNo)}'";

        var records = await SafeFetchAsync<PostedDefermentRecord>(
            BuildUrl(_postedDefermentEntity, [filter]),
            cancellationToken);

        return records.Select(MapToDto).ToList();
    }

    public async Task<List<ExamBookingDto>> GetExamBookingsAsync(
        string customerNo,
        string? registrationNo = null,
        CancellationToken cancellationToken = default)
    {
        List<string> regNos = !string.IsNullOrWhiteSpace(registrationNo)
            ? new List<string> { registrationNo }
            : await GetStudentRegistrationNosAsync(customerNo, cancellationToken);

        string filter = regNos.Count > 0
            ? string.Join(" or ", regNos.Select(r => $"Student_Reg_No eq '{ODataEscape(r)}'"))
            : $"Student_No eq '{ODataEscape(customerNo)}'";

        var records = await SafeFetchAsync<StudentExamBookingRecord>(
            BuildUrl(_studentsExamBookingsEntity, [filter]),
            cancellationToken);

        return records.Select(MapToDto).ToList();
    }

    public async Task<List<ProcessedBookingDto>> GetProcessedBookingsAsync(
        string customerNo,
        string? registrationNo = null,
        CancellationToken cancellationToken = default)
    {
        List<string> regNos = !string.IsNullOrWhiteSpace(registrationNo)
            ? new List<string> { registrationNo }
            : await GetStudentRegistrationNosAsync(customerNo, cancellationToken);

        string filter = regNos.Count > 0
            ? string.Join(" or ", regNos.Select(r => $"Student_Reg_No eq '{ODataEscape(r)}'"))
            : $"Student_No eq '{ODataEscape(customerNo)}'";

        var records = await SafeFetchAsync<ProcessedBookingRecord>(
            BuildUrl(_processedBookingsEntity, [filter]),
            cancellationToken);

        return records.Select(MapToDto).ToList();
    }

    public async Task<List<ExamResultDto>> GetExamResultsAsync(
        string customerNo,
        string? registrationNo = null,
        CancellationToken cancellationToken = default)
    {
        List<string> regNos = !string.IsNullOrWhiteSpace(registrationNo)
            ? new List<string> { registrationNo }
            : await GetStudentRegistrationNosAsync(customerNo, cancellationToken);

        string filter = regNos.Count > 0
            ? string.Join(" or ", regNos.Select(r => $"Student_Reg_No eq '{ODataEscape(r)}'"))
            : $"Student_No eq '{ODataEscape(customerNo)}'";

        var records = await SafeFetchAsync<ExamResultRecord>(
            BuildUrl(_examResultsEntity, [filter]),
            cancellationToken);

        // Fallback: try Registration_No if empty
        if (records.Count == 0 && regNos.Count > 0)
        {
            var fallbackFilter = string.Join(" or ", regNos.Select(r => $"Registration_No eq '{ODataEscape(r)}'"));
            records = await SafeFetchAsync<ExamResultRecord>(
                BuildUrl(_examResultsEntity, [fallbackFilter]),
                cancellationToken);
        }

        return records.Select(MapToDto).ToList();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Parallel orchestrator for full profile (Two-Phase Linkage)
    // ──────────────────────────────────────────────────────────────────────────

    private async Task<StudentProfile> FetchAndBuildCompleteProfileAsync(
        StudentListRecord student,
        string customerNo,
        string? knownRegistrationNo,
        CancellationToken cancellationToken)
    {
        // ── Phase 1: Fetch ExamAccounts and Ledger first ──
        var examTask = SafeFetchAsync<ExamAccount>(
            BuildUrl(_examAccountsEntity, [$"Student_Cust_No eq '{ODataEscape(customerNo)}'"]), cancellationToken);

        var ledgerTask = SafeFetchAsync<LedgerEntry>(
            BuildUrl(_customerEntriesEntity, [$"Customer_No eq '{ODataEscape(customerNo)}'"]), cancellationToken);

        await Task.WhenAll(examTask, ledgerTask);

        var examAccounts = await examTask;
        var ledgerEntries = await ledgerTask;

        // Extract all registration numbers for this student across all courses
        var regNos = examAccounts
            .Select(e => e.RegistrationNo)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!string.IsNullOrWhiteSpace(knownRegistrationNo) && !regNos.Contains(knownRegistrationNo, StringComparer.OrdinalIgnoreCase))
        {
            regNos.Add(knownRegistrationNo);
        }

        var primaryReg = knownRegistrationNo
            ?? examAccounts.FirstOrDefault(e => string.Equals(e.Status, "Active", StringComparison.OrdinalIgnoreCase))?.RegistrationNo
            ?? examAccounts.FirstOrDefault()?.RegistrationNo
            ?? string.Empty;

        // ── Phase 2: Link all academic services by Registration Number ──
        string regFilter = regNos.Count > 0
            ? string.Join(" or ", regNos.Select(r => $"Student_Reg_No eq '{ODataEscape(r!)}'"))
            : $"Student_No eq '{ODataEscape(customerNo)}'";

        string exemptionRegFilter = regNos.Count > 0
            ? string.Join(" or ", regNos.Select(r => $"Stud_Reg_No eq '{ODataEscape(r!)}'"))
            : $"Stud_Cust_No eq '{ODataEscape(customerNo)}'";

        _logger.LogInformation("Fetching academic data for student {CustomerNo} using reg filter: [{RegFilter}]",
            customerNo, regFilter);

        var exemptionTask = SafeFetchAsync<ExemptionEntryRecord>(
            BuildUrl(_exemptionEntriesEntity, [$"({exemptionRegFilter}) and Remove eq false"]), cancellationToken);

        var defermentTask = SafeFetchAsync<PostedDefermentRecord>(
            BuildUrl(_postedDefermentEntity, [regFilter]), cancellationToken);

        var bookingTask = SafeFetchAsync<StudentExamBookingRecord>(
            BuildUrl(_studentsExamBookingsEntity, [regFilter]), cancellationToken);

        var processedTask = SafeFetchAsync<ProcessedBookingRecord>(
            BuildUrl(_processedBookingsEntity, [regFilter]), cancellationToken);

        var resultsTask = SafeFetchAsync<ExamResultRecord>(
            BuildUrl(_examResultsEntity, [regFilter]), cancellationToken);

        await Task.WhenAll(exemptionTask, defermentTask, bookingTask, processedTask, resultsTask);

        var exemptions = await exemptionTask;
        var deferments = await defermentTask;
        var bookings = await bookingTask;
        var processedBookings = await processedTask;
        var examResultsRecords = await resultsTask;

        // Fallback for ExamResults: try Registration_No if Student_Reg_No yielded 0
        if (examResultsRecords.Count == 0 && regNos.Count > 0)
        {
            var fallbackFilter = string.Join(" or ", regNos.Select(r => $"Registration_No eq '{ODataEscape(r!)}'"));
            examResultsRecords = await SafeFetchAsync<ExamResultRecord>(
                BuildUrl(_examResultsEntity, [fallbackFilter]), cancellationToken);
        }

        var examResults = examResultsRecords.Select(MapToDto).ToList();

        return BuildProfile(student, examAccounts, ledgerEntries, exemptions, deferments, bookings, processedBookings, examResults);
    }

    private static StudentProfile BuildProfile(
        StudentListRecord student,
        List<ExamAccount> examAccounts,
        List<LedgerEntry> ledgerEntries,
        List<ExemptionEntryRecord> exemptions,
        List<PostedDefermentRecord> deferments,
        List<StudentExamBookingRecord> bookings,
        List<ProcessedBookingRecord> processedBookings,
        List<ExamResultDto> examResults)
    {
        var primaryExam = examAccounts.FirstOrDefault(e => string.Equals(e.Status, "Active", StringComparison.OrdinalIgnoreCase))
                       ?? examAccounts.FirstOrDefault();

        var courseTitle = !string.IsNullOrWhiteSpace(primaryExam?.CourseDescription)
            ? primaryExam.CourseDescription
            : (primaryExam?.CourseId ?? string.Empty);

        return new StudentProfile
        {
            CustomerNo            = student.No ?? string.Empty,
            PrimaryRegistrationNo = primaryExam?.RegistrationNo ?? string.Empty,
            QualificationPathway  = courseTitle,
            Name                  = student.Name    ?? string.Empty,
            IdNo        = student.IdNo    ?? string.Empty,
            PhoneNo     = student.PhoneNo ?? string.Empty,
            Email       = student.Email   ?? string.Empty,
            Gender      = student.Gender  ?? string.Empty,
            Address     = student.Address ?? string.Empty,
            City        = student.City    ?? string.Empty,
            Disabled    = student.Disabled,
            Balance     = student.Balance,
            BalanceLcy  = student.BalanceLcy,
            SalesLcy    = student.SalesLcy,

            ExamAccounts = examAccounts.Select(e => new ExamAccountDto
            {
                RegistrationNo   = e.RegistrationNo   ?? string.Empty,
                RegistrationDate = e.RegistrationDate ?? string.Empty,
                FirstName        = e.FirstName        ?? string.Empty,
                MiddleName       = e.MiddleName       ?? string.Empty,
                Surname          = e.Surname          ?? string.Empty,
                CourseId         = e.CourseId         ?? string.Empty,
                CourseDescription= e.CourseDescription?? string.Empty,
                Status           = e.Status           ?? string.Empty,
                StatusRemarks    = e.StatusRemarks    ?? string.Empty,
                Blocked          = e.Blocked,
                BlockingRemarks  = e.BlockingRemarks  ?? string.Empty,
                Balance          = e.Balance,
                RenewalAmount    = e.RenewalAmount,
                ReActivationAmount = e.ReActivationAmount,
                RenewalPending   = e.RenewalPending,
                KasnebFoundation = e.KasnebFoundation,
                TotalAmountFromHelb = e.TotalAmountFromHelb,
                LastExamDate     = e.LastExamDate     ?? string.Empty,
                LastPaymentDate  = e.LastPaymentDate  ?? string.Empty
            }).ToList(),

            Exemptions = exemptions.Select(MapToDto).ToList(),
            Deferments = deferments.Select(MapToDto).ToList(),
            ExamBookings = bookings.Select(MapToDto).ToList(),
            ProcessedBookings = processedBookings.Select(MapToDto).ToList(),
            ExamResults = examResults,

            LedgerEntries = ledgerEntries
                .OrderByDescending(l => l.EntryNo)
                .Select(l => new LedgerEntryDto
                {
                    EntryNo            = l.EntryNo,
                    PostingDate        = l.PostingDate        ?? string.Empty,
                    DocumentType       = l.DocumentType       ?? string.Empty,
                    DocumentNo         = l.DocumentNo         ?? string.Empty,
                    RegistrationNo     = l.RegistrationNo     ?? string.Empty,
                    Description        = l.Description        ?? string.Empty,
                    OriginalAmount     = l.OriginalAmount,
                    Amount             = l.Amount,
                    DebitAmount        = l.DebitAmount,
                    CreditAmount       = l.CreditAmount,
                    RemainingAmount    = l.RemainingAmount,
                    DueDate            = l.DueDate            ?? string.Empty,
                    PaymentMethodCode  = l.PaymentMethodCode  ?? string.Empty,
                    Open               = l.Open,
                    ExternalDocumentNo = l.ExternalDocumentNo ?? string.Empty,
                    Reversed           = l.Reversed
                }).ToList()
        };
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Mapping Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static ExemptionDto MapToDto(ExemptionEntryRecord r) => new()
    {
        EntryNo = r.EntryNo,
        StudCustNo = r.StudCustNo ?? string.Empty,
        StudRegNo = r.StudRegNo ?? string.Empty,
        ExemptionVoucherNo = r.ExemptionVoucherNo ?? string.Empty,
        CourseId = r.CourseId ?? string.Empty,
        Type = r.Type ?? string.Empty,
        Level = r.Level ?? string.Empty,
        PaperNo = r.No ?? string.Empty,
        PaperName = r.Name ?? string.Empty,
        CurrencyCode = r.CurrencyCode ?? string.Empty,
        Amount = r.Amount,
        AmountLcy = r.AmountLcy,
        LastDateModified = r.LastDateModified ?? string.Empty
    };

    private static DefermentDto MapToDto(PostedDefermentRecord r) => new()
    {
        DefermentNo = r.No ?? string.Empty,
        Date = r.Date ?? string.Empty,
        StudentNo = r.StudentNo ?? string.Empty,
        StudentRegNo = r.StudentRegNo ?? string.Empty,
        ExaminationId = r.ExaminationId ?? string.Empty,
        ExaminationDescription = r.ExaminationDescription ?? string.Empty,
        ExaminationSitting = r.ExaminationSitting ?? string.Empty,
        PreferredExaminationSitting = r.PreferredExaminationSitting ?? string.Empty,
        CreatedBy = r.CreatedBy ?? string.Empty,
        CreatedOn = r.CreatedOn ?? string.Empty,
        PostedBy = r.PostedBy ?? string.Empty,
        PostedOn = r.PostedOn ?? string.Empty
    };

    private static ExamBookingDto MapToDto(StudentExamBookingRecord r) => new()
    {
        BookingNo = r.No ?? string.Empty,
        Date = r.Date ?? string.Empty,
        StudentNo = r.StudentNo ?? string.Empty,
        StudentRegNo = r.StudentRegNo ?? string.Empty,
        ExaminationId = r.ExaminationId ?? string.Empty,
        ExaminationDescription = r.ExaminationDescription ?? string.Empty,
        ExaminationSitting = r.ExaminationSitting ?? string.Empty,
        BookingReceiptNo = r.BookingReceiptNo ?? string.Empty,
        BookingInvoiceNo = r.BookingInvoiceNo ?? string.Empty,
        CreatedBy = r.CreatedBy ?? string.Empty,
        CreatedOn = r.CreatedOn ?? string.Empty
    };

    private static ProcessedBookingDto MapToDto(ProcessedBookingRecord r) => new()
    {
        BookingNo = r.No ?? string.Empty,
        Date = r.Date ?? string.Empty,
        StudentNo = r.StudentNo ?? string.Empty,
        StudentRegNo = r.StudentRegNo ?? string.Empty,
        ExaminationId = r.ExaminationId ?? string.Empty,
        ExaminationDescription = r.ExaminationDescription ?? string.Empty,
        BookingAmount = r.BookingAmount,
        ExaminationCenterCode = r.ExaminationCenterCode ?? string.Empty,
        ExaminationCenter = r.ExaminationCenter ?? string.Empty,
        PhoneNo = r.PhoneNo ?? string.Empty,
        Gender = r.Gender ?? string.Empty,
        Disabled = r.Disabled,
        CreatedBy = r.CreatedBy ?? string.Empty,
        CreatedOn = r.CreatedOn ?? string.Empty,
        PostedBy = r.PostedBy ?? string.Empty,
        PostedOn = r.PostedOn ?? string.Empty
    };

    private static ExamResultDto MapToDto(ExamResultRecord r) => new()
    {
        LineNo = r.LineNo,
        Examination = r.Examination ?? string.Empty,
        Part = r.Part ?? string.Empty,
        Section = r.Section ?? string.Empty,
        Paper = r.Paper ?? string.Empty,
        PaperName = r.PaperName ?? string.Empty,
        FinancialYear = r.FinancialYear ?? string.Empty,
        Grade = r.Grade ?? string.Empty,
        SectionGrade = r.SectionGrade ?? string.Empty,
        SectionDescription = r.SectionDescription ?? string.Empty,
        ExaminationSittingId = r.ExaminationSittingId ?? string.Empty,
        ExaminationCenter = r.ExaminationCenter ?? string.Empty,
        Mark = r.Mark,
        Passed = r.Passed,
        Remarks = r.Remarks ?? string.Empty
    };

    private async Task<List<T>> SafeFetchAsync<T>(string url, CancellationToken cancellationToken)
    {
        try
        {
            return await _fetcher.FetchAllAsync<T>(url, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch from {Url} — error: {Message}", url, ex.Message);
            return new List<T>();
        }
    }

    private static string BuildUrl(string entity, IEnumerable<string>? filterConditions = null)
    {
        var conditions = filterConditions?.Where(f => !string.IsNullOrWhiteSpace(f)).ToList();

        if (conditions is { Count: > 0 })
            return $"{entity}?$filter={string.Join(" and ", conditions)}";

        return entity;
    }

    private static string ODataEscape(string value) => value.Replace("'", "''");
}
