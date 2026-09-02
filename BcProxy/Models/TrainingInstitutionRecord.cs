using System.Text.Json.Serialization;

namespace BcProxy.Models;

/// <summary>
/// Maps directly to a row in the Business Central Training_Institutions OData entity.
/// </summary>
public class TrainingInstitutionRecord
{
    [JsonPropertyName("No")]
    public string? No { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Address")]
    public string? Address { get; set; }

    [JsonPropertyName("Address_2")]
    public string? Address2 { get; set; }

    [JsonPropertyName("City")]
    public string? City { get; set; }

    [JsonPropertyName("Contact")]
    public string? Contact { get; set; }

    [JsonPropertyName("Phone_No")]
    public string? PhoneNo { get; set; }

    [JsonPropertyName("Gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("Accreditation_Status")]
    public string? AccreditationStatus { get; set; }

    [JsonPropertyName("Date_of_Birth")]
    public string? DateOfBirth { get; set; }

    [JsonPropertyName("Disabled")]
    public bool Disabled { get; set; }

    [JsonPropertyName("Accreditation_Start_Date")]
    public string? AccreditationStartDate { get; set; }

    [JsonPropertyName("Accreditation_End_Date")]
    public string? AccreditationEndDate { get; set; }

    [JsonPropertyName("NCPWD_No")]
    public string? NcpwdNo { get; set; }
}
