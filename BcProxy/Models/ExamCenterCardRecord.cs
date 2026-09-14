using System.Text.Json.Serialization;

namespace BcProxy.Models;

/// <summary>
/// Maps directly to a row in the Business Central ExamCenterCard OData entity.
/// </summary>
public class ExamCenterCardRecord
{
    [JsonPropertyName("Code")]
    public string? Code { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Exam_Zone")]
    public string? ExamZone { get; set; }

    [JsonPropertyName("Exam_Region")]
    public string? ExamRegion { get; set; }

    [JsonPropertyName("Exam_Route")]
    public string? ExamRoute { get; set; }

    [JsonPropertyName("Disability_Friendly")]
    public bool DisabilityFriendly { get; set; }

    [JsonPropertyName("Computer_Based_Friendly")]
    public bool ComputerBasedFriendly { get; set; }

    [JsonPropertyName("KISEB_Centers")]
    public bool KisebCenters { get; set; }

    [JsonPropertyName("Exam_Buffer_Zone")]
    public bool ExamBufferZone { get; set; }

    [JsonPropertyName("Neutral_Buffer_Zone")]
    public bool NeutralBufferZone { get; set; }

    [JsonPropertyName("Blocked")]
    public bool Blocked { get; set; }

    [JsonPropertyName("Training_Institution")]
    public string? TrainingInstitution { get; set; }

    [JsonPropertyName("Institution")]
    public string? Institution { get; set; }

    [JsonPropertyName("Type")]
    public string? Type { get; set; }

    [JsonPropertyName("Status")]
    public string? Status { get; set; }

    [JsonPropertyName("Maximum_Capacity_Per_Session")]
    public int? MaximumCapacityPerSession { get; set; }

    [JsonPropertyName("Total_Booked_Students")]
    public int? TotalBookedStudents { get; set; }

    [JsonPropertyName("Address")]
    public string? Address { get; set; }

    [JsonPropertyName("Address_2")]
    public string? Address2 { get; set; }

    [JsonPropertyName("Post_Code")]
    public string? PostCode { get; set; }

    [JsonPropertyName("City")]
    public string? City { get; set; }

    [JsonPropertyName("Country_Region_Code")]
    public string? CountryRegionCode { get; set; }

    [JsonPropertyName("ShowMap")]
    public string? ShowMap { get; set; }

    [JsonPropertyName("Contact")]
    public string? Contact { get; set; }

    [JsonPropertyName("Phone_No")]
    public string? PhoneNo { get; set; }

    [JsonPropertyName("Fax_No")]
    public string? FaxNo { get; set; }

    [JsonPropertyName("E_Mail")]
    public string? Email { get; set; }

    [JsonPropertyName("Home_Page")]
    public string? HomePage { get; set; }
}
