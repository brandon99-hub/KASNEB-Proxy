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
/// Full student profile returned by GET /students/{customerNo}.
/// Combines bio-data (Studentlist) + exam accounts (ExamAccounts) + ledger (customerEntries)
/// all scoped to a single student, fetched in parallel for performance.
/// </summary>
public class StudentProfile
{
    /// <summary>BC Customer No — primary identifier</summary>
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

    /// <summary>
    /// All KASNEB exam / registration accounts for this student.
    /// A student may be registered in multiple courses (e.g. CPA + CIFA).
    /// </summary>
    public List<ExamAccountDto> ExamAccounts { get; set; } = new();

    /// <summary>
    /// All posted customer ledger entries (payments, invoices, credit memos)
    /// for this student, sorted newest-first.
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
