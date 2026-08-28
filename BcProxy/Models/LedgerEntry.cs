using System.Text.Json.Serialization;

namespace BcProxy.Models;

/// <summary>
/// Maps directly to a row in the Business Central customerEntries OData entity.
/// Each row is one posted customer ledger entry (payment, invoice, credit memo, etc.)
/// for a given student.
/// </summary>
public class LedgerEntry
{
    [JsonPropertyName("Entry_No")]
    public int EntryNo { get; set; }

    [JsonPropertyName("Posting_Date")]
    public string? PostingDate { get; set; }

    [JsonPropertyName("Document_Type")]
    public string? DocumentType { get; set; }

    [JsonPropertyName("Document_No")]
    public string? DocumentNo { get; set; }

    /// <summary>FK to Studentlist.No</summary>
    [JsonPropertyName("Customer_No")]
    public string? CustomerNo { get; set; }

    /// <summary>KASNEB Registration No associated with this entry (may be empty)</summary>
    [JsonPropertyName("Registration_No")]
    public string? RegistrationNo { get; set; }

    [JsonPropertyName("Customer_Name")]
    public string? CustomerName { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("Original_Amount")]
    public decimal OriginalAmount { get; set; }

    [JsonPropertyName("Amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("Debit_Amount")]
    public decimal DebitAmount { get; set; }

    [JsonPropertyName("Credit_Amount")]
    public decimal CreditAmount { get; set; }

    [JsonPropertyName("Remaining_Amount")]
    public decimal RemainingAmount { get; set; }

    [JsonPropertyName("Sales_LCY")]
    public decimal SalesLcy { get; set; }

    [JsonPropertyName("Due_Date")]
    public string? DueDate { get; set; }

    [JsonPropertyName("Payment_Method_Code")]
    public string? PaymentMethodCode { get; set; }

    [JsonPropertyName("Open")]
    public bool Open { get; set; }

    [JsonPropertyName("On_Hold")]
    public string? OnHold { get; set; }

    [JsonPropertyName("External_Document_No")]
    public string? ExternalDocumentNo { get; set; }

    [JsonPropertyName("Reversed")]
    public bool Reversed { get; set; }

    [JsonPropertyName("Reversed_by_Entry_No")]
    public int ReversedByEntryNo { get; set; }

    [JsonPropertyName("Reversed_Entry_No")]
    public int ReversedEntryNo { get; set; }
}
