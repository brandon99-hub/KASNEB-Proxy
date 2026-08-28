using BcProxy.Models;
using System.Net;

namespace BcProxy.Services;

/// <summary>
/// Core service for KASNEB student stakeholder data.
///
/// Orchestrates three BC OData web services:
///   1. Studentlist       — bio-data (No, ID_No, Phone_No, E_Mail, Gender, Balance)
///   2. ExamAccounts      — KASNEB registration accounts (Registration_No, Course, Status, Renewal)
///   3. customerEntries   — posted ledger entries (payments, invoices, credit memos)
///
/// All paging is handled transparently by ODataFetcher using @odata.nextLink continuation.
/// </summary>
public class StudentProfileService
{
    private readonly ODataFetcher _fetcher;
    private readonly ILogger<StudentProfileService> _logger;
    private readonly string _studentListEntity;
    private readonly string _examAccountsEntity;
    private readonly string _customerEntriesEntity;

    public StudentProfileService(
        ODataFetcher fetcher,
        ILogger<StudentProfileService> logger,
        IConfiguration configuration)
    {
        _fetcher = fetcher;
        _logger = logger;

        _studentListEntity = configuration["BusinessCentral:StudentListEntity"]
            ?? throw new InvalidOperationException("BusinessCentral:StudentListEntity is not configured");
        _examAccountsEntity = configuration["BusinessCentral:ExamAccountsEntity"]
            ?? throw new InvalidOperationException("BusinessCentral:ExamAccountsEntity is not configured");
        _customerEntriesEntity = configuration["BusinessCentral:CustomerEntriesEntity"]
            ?? throw new InvalidOperationException("BusinessCentral:CustomerEntriesEntity is not configured");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // LIST — bio-data only (for CRM student list page)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all students from Studentlist as lightweight summaries (bio-data only).
    /// Uses full pagination to guarantee no students are missed.
    /// Optionally filters server-side by name or ID number.
    /// </summary>
    public async Task<List<StudentSummary>> GetAllStudentsAsync(
        string? nameFilter = null,
        string? idNoFilter = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching student list — name filter: '{Name}', idNo filter: '{IdNo}'",
            nameFilter, idNoFilter);

        var filterConditions = new List<string>();

        if (!string.IsNullOrWhiteSpace(nameFilter))
            filterConditions.Add($"contains(Name, '{ODataEscape(nameFilter)}')");

        if (!string.IsNullOrWhiteSpace(idNoFilter))
            filterConditions.Add($"ID_No eq '{ODataEscape(idNoFilter)}'");

        var url = BuildUrl(_studentListEntity, filterConditions);
        var records = await _fetcher.FetchAllAsync<StudentListRecord>(url, cancellationToken);

        _logger.LogInformation("StudentList: {Count} records returned", records.Count);

        return records.Select(r => new StudentSummary
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
        .OrderBy(s => s.Name)
        .ToList();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // DETAIL — full profile for a single student (bio + exam accounts + ledger)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the full profile for a student identified by their BC Customer No.
    /// Fetches bio-data, exam accounts, and ledger entries in parallel.
    /// </summary>
    public async Task<StudentProfile?> GetStudentByCustomerNoAsync(
        string customerNo,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching full profile for Customer No: {CustomerNo}", customerNo);

        var (bioData, examAccounts, ledgerEntries) = await FetchProfileDataAsync(
            studentListFilter: $"No eq '{ODataEscape(customerNo)}'",
            examAccountsFilter: $"Student_Cust_No eq '{ODataEscape(customerNo)}'",
            ledgerEntriesFilter: $"Customer_No eq '{ODataEscape(customerNo)}'",
            cancellationToken);

        var student = bioData.FirstOrDefault();
        if (student is null)
        {
            _logger.LogWarning("No Studentlist record found for Customer No: {CustomerNo}", customerNo);
            return null;
        }

        return BuildProfile(student, examAccounts, ledgerEntries);
    }

    /// <summary>
    /// Returns the full profile for a student identified by their National ID number.
    /// </summary>
    public async Task<StudentProfile?> GetStudentByIdNoAsync(
        string idNo,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching full profile for ID No: {IdNo}", idNo);

        // Step 1: resolve Customer No from Studentlist via ID_No
        var studentListUrl = BuildUrl(_studentListEntity, [$"ID_No eq '{ODataEscape(idNo)}'"]);
        var bioList = await _fetcher.FetchAllAsync<StudentListRecord>(studentListUrl, cancellationToken);
        var student = bioList.FirstOrDefault();

        if (student is null || string.IsNullOrEmpty(student.No))
        {
            _logger.LogWarning("No Studentlist record found for ID No: {IdNo}", idNo);
            return null;
        }

        var customerNo = student.No;

        // Step 2: fetch exam accounts + ledger in parallel now that we have Customer No
        var examTask = _fetcher.FetchAllAsync<ExamAccount>(
            BuildUrl(_examAccountsEntity, [$"Student_Cust_No eq '{ODataEscape(customerNo)}'"]),
            cancellationToken);

        var ledgerTask = _fetcher.FetchAllAsync<LedgerEntry>(
            BuildUrl(_customerEntriesEntity, [$"Customer_No eq '{ODataEscape(customerNo)}'"]),
            cancellationToken);

        await Task.WhenAll(examTask, ledgerTask);

        return BuildProfile(student, await examTask, await ledgerTask);
    }

    /// <summary>
    /// Returns the full profile for a student identified by their KASNEB Registration No.
    /// Looks up via ExamAccounts, then resolves the Customer No.
    /// </summary>
    public async Task<StudentProfile?> GetStudentByRegistrationNoAsync(
        string registrationNo,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching full profile for Registration No: {RegistrationNo}", registrationNo);

        // Step 1: resolve Customer No from ExamAccounts
        var examUrl = BuildUrl(_examAccountsEntity, [$"Registration_No eq '{ODataEscape(registrationNo)}'"]);
        var examList = await _fetcher.FetchAllAsync<ExamAccount>(examUrl, cancellationToken);

        var primaryExam = examList.FirstOrDefault();
        if (primaryExam is null || string.IsNullOrEmpty(primaryExam.StudentCustNo))
        {
            _logger.LogWarning("No ExamAccount found for Registration No: {RegistrationNo}", registrationNo);
            return null;
        }

        var customerNo = primaryExam.StudentCustNo;

        // Step 2: fetch bio-data + all exam accounts + ledger in parallel
        var bioTask = _fetcher.FetchAllAsync<StudentListRecord>(
            BuildUrl(_studentListEntity, [$"No eq '{ODataEscape(customerNo)}'"]),
            cancellationToken);

        // Fetch ALL exam accounts for this student (not just the one we found)
        var allExamTask = _fetcher.FetchAllAsync<ExamAccount>(
            BuildUrl(_examAccountsEntity, [$"Student_Cust_No eq '{ODataEscape(customerNo)}'"]),
            cancellationToken);

        var ledgerTask = _fetcher.FetchAllAsync<LedgerEntry>(
            BuildUrl(_customerEntriesEntity, [$"Customer_No eq '{ODataEscape(customerNo)}'"]),
            cancellationToken);

        await Task.WhenAll(bioTask, allExamTask, ledgerTask);

        var student = (await bioTask).FirstOrDefault();
        if (student is null)
        {
            _logger.LogWarning("No Studentlist bio-data found for Customer No: {CustomerNo}", customerNo);
            return null;
        }

        return BuildProfile(student, await allExamTask, await ledgerTask);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Fires three BC OData calls in parallel using Task.WhenAll for minimum latency.
    /// </summary>
    private async Task<(List<StudentListRecord> bio, List<ExamAccount> exams, List<LedgerEntry> ledger)>
        FetchProfileDataAsync(
            string studentListFilter,
            string examAccountsFilter,
            string ledgerEntriesFilter,
            CancellationToken cancellationToken)
    {
        var bioTask = _fetcher.FetchAllAsync<StudentListRecord>(
            BuildUrl(_studentListEntity, [studentListFilter]), cancellationToken);

        var examTask = _fetcher.FetchAllAsync<ExamAccount>(
            BuildUrl(_examAccountsEntity, [examAccountsFilter]), cancellationToken);

        var ledgerTask = _fetcher.FetchAllAsync<LedgerEntry>(
            BuildUrl(_customerEntriesEntity, [ledgerEntriesFilter]), cancellationToken);

        await Task.WhenAll(bioTask, examTask, ledgerTask);

        return (await bioTask, await examTask, await ledgerTask);
    }

    /// <summary>
    /// Assembles a StudentProfile from the three raw entity lists.
    /// </summary>
    private static StudentProfile BuildProfile(
        StudentListRecord student,
        List<ExamAccount> examAccounts,
        List<LedgerEntry> ledgerEntries)
    {
        return new StudentProfile
        {
            CustomerNo  = student.No      ?? string.Empty,
            Name        = student.Name    ?? string.Empty,
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

    /// <summary>
    /// Builds an OData URL with optional $filter conditions.
    /// </summary>
    private static string BuildUrl(string entity, IEnumerable<string>? filterConditions = null)
    {
        var conditions = filterConditions?.Where(f => !string.IsNullOrWhiteSpace(f)).ToList();

        if (conditions is { Count: > 0 })
            return $"{entity}?$filter={string.Join(" and ", conditions)}";

        return entity;
    }

    /// <summary>
    /// Escapes single quotes in OData string literals to prevent injection / malformed queries.
    /// OData escaping rule: replace ' with ''
    /// </summary>
    private static string ODataEscape(string value) => value.Replace("'", "''");
}
