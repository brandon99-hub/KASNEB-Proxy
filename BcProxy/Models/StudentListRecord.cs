using System.Text.Json.Serialization;

namespace BcProxy.Models;

/// <summary>
/// Maps directly to a row in the Business Central Studentlist OData entity.
/// Contains general bio-data for a student/customer. The "No" field is the
/// primary key and is the join key to ExamAccounts and customerEntries.
/// </summary>
public class StudentListRecord
{
    /// <summary>BC Customer No — primary join key (e.g. "ST00145181")</summary>
    [JsonPropertyName("No")]
    public string? No { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("ID_No")]
    public string? IdNo { get; set; }

    [JsonPropertyName("Phone_No")]
    public string? PhoneNo { get; set; }

    [JsonPropertyName("Address")]
    public string? Address { get; set; }

    [JsonPropertyName("Address_2")]
    public string? Address2 { get; set; }

    [JsonPropertyName("E_Mail")]
    public string? Email { get; set; }

    [JsonPropertyName("City")]
    public string? City { get; set; }

    [JsonPropertyName("Contact")]
    public string? Contact { get; set; }

    [JsonPropertyName("Gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("Disabled")]
    public bool Disabled { get; set; }

    [JsonPropertyName("Balance")]
    public decimal Balance { get; set; }

    [JsonPropertyName("Balance_LCY")]
    public decimal BalanceLcy { get; set; }

    [JsonPropertyName("Sales_LCY")]
    public decimal SalesLcy { get; set; }
}
