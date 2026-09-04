using System.Text.Json.Serialization;

namespace BcProxy.Models;

/// <summary>
/// Maps to a row in the Business Central ExemptionEntries OData entity.
/// </summary>
public class ExemptionEntryRecord
{
    [JsonPropertyName("Entry_No")]
    public int EntryNo { get; set; }

    [JsonPropertyName("Remove")]
    public bool Remove { get; set; }

    [JsonPropertyName("Stud_Cust_No")]
    public string? StudCustNo { get; set; }

    [JsonPropertyName("Stud_Reg_No")]
    public string? StudRegNo { get; set; }

    [JsonPropertyName("Student_Name")]
    public string? StudentName { get; set; }

    [JsonPropertyName("Exemption_Voucher_No")]
    public string? ExemptionVoucherNo { get; set; }

    [JsonPropertyName("Course_Id")]
    public string? CourseId { get; set; }

    [JsonPropertyName("Type")]
    public string? Type { get; set; }

    [JsonPropertyName("Level")]
    public string? Level { get; set; }

    [JsonPropertyName("No")]
    public string? No { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Currency_Code")]
    public string? CurrencyCode { get; set; }

    [JsonPropertyName("Amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("Amount_LCY")]
    public decimal AmountLcy { get; set; }

    [JsonPropertyName("Last_Date_Modified")]
    public string? LastDateModified { get; set; }
}
