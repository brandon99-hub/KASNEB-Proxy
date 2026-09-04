namespace BcProxy.Models;

/// <summary>
/// Generic paginated envelope for list endpoints.
/// </summary>
public class PagedResponse<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Count { get; set; }
    public List<T> Data { get; set; } = new();
}

/// <summary>
/// Lightweight student summary returned by GET /students (list view).
/// Contains bio-data from Studentlist only — no exam accounts or ledger entries.
/// Designed for CRM list pages where the full profile is not yet needed.
/// </summary>
public class StudentSummary
{
    /// <summary>BC Customer No — the unique internal identifier (e.g. "ST00145181")</summary>
    public string CustomerNo { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string IdNo { get; set; } = string.Empty;

    public string PhoneNo { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Gender { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public bool Disabled { get; set; }

    public decimal Balance { get; set; }

    public decimal BalanceLcy { get; set; }

    public decimal SalesLcy { get; set; }
}

/// <summary>
/// Full 360-degree student profile returned by GET /students/{customerNo}.
/// Combines bio-data, exam accounts, exemptions, deferments, exam bookings,
/// processed bookings, exam results, and ledger entries.
/// </summary>
public class StudentProfile
{
    /// <summary>BC Customer No — primary internal ERP identifier (e.g. "ST00000453")</summary>
    public string CustomerNo { get; set; } = string.Empty;

    /// <summary>Primary KASNEB Registration No from student's Exam Accounts (e.g. "NAC/181912")</summary>
    public string PrimaryRegistrationNo { get; set; } = string.Empty;

    /// <summary>Primary Qualification Pathway / Course title (e.g. "Certified Public Accountants")</summary>
    public string QualificationPathway { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string IdNo { get; set; } = string.Empty;

    public string PhoneNo { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Gender { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public bool Disabled { get; set; }

    public decimal Balance { get; set; }

    public decimal BalanceLcy { get; set; }

    public decimal SalesLcy { get; set; }

    /// <summary>
    /// All KASNEB exam / registration accounts for this student.
    /// </summary>
    public List<ExamAccountDto> ExamAccounts { get; set; } = new();

    /// <summary>
    /// All granted paper exemptions for this student.
    /// </summary>
    public List<ExemptionDto> Exemptions { get; set; } = new();

    /// <summary>
    /// All posted exam sitting deferrals.
    /// </summary>
    public List<DefermentDto> Deferments { get; set; } = new();

    /// <summary>
    /// All student exam bookings/applications.
    /// </summary>
    public List<ExamBookingDto> ExamBookings { get; set; } = new();

    /// <summary>
    /// Confirmed exam bookings with allocated exam centers.
    /// </summary>
    public List<ProcessedBookingDto> ProcessedBookings { get; set; } = new();

    /// <summary>
    /// Academic exam performance, grades, and marks history.
    /// </summary>
    public List<ExamResultDto> ExamResults { get; set; } = new();

    /// <summary>
    /// All posted customer ledger entries (payments, invoices, credit memos) sorted newest-first.
    /// </summary>
    public List<LedgerEntryDto> LedgerEntries { get; set; } = new();
}

// ─── Clean DTO shapes (strips OData internals) ──────────────────────────────

public class ExamAccountDto
{
    public string RegistrationNo { get; set; } = string.Empty;
    public string RegistrationDate { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string CourseDescription { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusRemarks { get; set; } = string.Empty;
    public bool Blocked { get; set; }
    public string BlockingRemarks { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal RenewalAmount { get; set; }
    public decimal ReActivationAmount { get; set; }
    public int RenewalPending { get; set; }
    public bool KasnebFoundation { get; set; }
    public decimal TotalAmountFromHelb { get; set; }
    public string LastExamDate { get; set; } = string.Empty;
    public string LastPaymentDate { get; set; } = string.Empty;
}

public class ExemptionDto
{
    public int EntryNo { get; set; }
    public string StudCustNo { get; set; } = string.Empty;
    public string StudRegNo { get; set; } = string.Empty;
    public string ExemptionVoucherNo { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string PaperNo { get; set; } = string.Empty;
    public string PaperName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal AmountLcy { get; set; }
    public string LastDateModified { get; set; } = string.Empty;
}

public class DefermentDto
{
    public string DefermentNo { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string StudentNo { get; set; } = string.Empty;
    public string StudentRegNo { get; set; } = string.Empty;
    public string ExaminationId { get; set; } = string.Empty;
    public string ExaminationDescription { get; set; } = string.Empty;
    public string ExaminationSitting { get; set; } = string.Empty;
    public string PreferredExaminationSitting { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedOn { get; set; } = string.Empty;
    public string PostedBy { get; set; } = string.Empty;
    public string PostedOn { get; set; } = string.Empty;
}

public class ExamBookingDto
{
    public string BookingNo { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string StudentNo { get; set; } = string.Empty;
    public string StudentRegNo { get; set; } = string.Empty;
    public string ExaminationId { get; set; } = string.Empty;
    public string ExaminationDescription { get; set; } = string.Empty;
    public string ExaminationSitting { get; set; } = string.Empty;
    public string BookingReceiptNo { get; set; } = string.Empty;
    public string BookingInvoiceNo { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedOn { get; set; } = string.Empty;
}

public class ProcessedBookingDto
{
    public string BookingNo { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string StudentNo { get; set; } = string.Empty;
    public string StudentRegNo { get; set; } = string.Empty;
    public string ExaminationId { get; set; } = string.Empty;
    public string ExaminationDescription { get; set; } = string.Empty;
    public decimal BookingAmount { get; set; }
    public string ExaminationCenterCode { get; set; } = string.Empty;
    public string ExaminationCenter { get; set; } = string.Empty;
    public string PhoneNo { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public bool Disabled { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedOn { get; set; } = string.Empty;
    public string PostedBy { get; set; } = string.Empty;
    public string PostedOn { get; set; } = string.Empty;
}

public class ExamResultDto
{
    public int LineNo { get; set; }
    public string Examination { get; set; } = string.Empty;
    public string Part { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string Paper { get; set; } = string.Empty;
    public string PaperName { get; set; } = string.Empty;
    public string FinancialYear { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public string SectionGrade { get; set; } = string.Empty;
    public string SectionDescription { get; set; } = string.Empty;
    public string ExaminationSittingId { get; set; } = string.Empty;
    public string ExaminationCenter { get; set; } = string.Empty;
    public decimal Mark { get; set; }
    public bool Passed { get; set; }
    public string Remarks { get; set; } = string.Empty;
}

public class LedgerEntryDto
{
    public int EntryNo { get; set; }
    public string PostingDate { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNo { get; set; } = string.Empty;
    public string RegistrationNo { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal OriginalAmount { get; set; }
    public decimal Amount { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string DueDate { get; set; } = string.Empty;
    public string PaymentMethodCode { get; set; } = string.Empty;
    public bool Open { get; set; }
    public string ExternalDocumentNo { get; set; } = string.Empty;
    public bool Reversed { get; set; }
}
