using System.Text.Json.Serialization;

namespace BcProxy.Models;

/// <summary>
/// Maps directly to a row in the Business Central VendorList OData entity.
/// Excludes financial/posting groups as requested.
/// </summary>
public class VendorListRecord
{
    [JsonPropertyName("No")]
    public string? No { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Search_Name")]
    public string? SearchName { get; set; }

    [JsonPropertyName("Responsibility_Center")]
    public string? ResponsibilityCenter { get; set; }

    [JsonPropertyName("Location_Code")]
    public string? LocationCode { get; set; }

    [JsonPropertyName("Post_Code")]
    public string? PostCode { get; set; }

    [JsonPropertyName("Country_Region_Code")]
    public string? CountryRegionCode { get; set; }

    [JsonPropertyName("Phone_No")]
    public string? PhoneNo { get; set; }

    [JsonPropertyName("Fax_No")]
    public string? FaxNo { get; set; }

    [JsonPropertyName("Email")]
    public string? Email { get; set; }

    [JsonPropertyName("E_Mail")]
    public string? EMail { get; set; }

    [JsonPropertyName("Contact")]
    public string? Contact { get; set; }

    [JsonPropertyName("Vendor_Type")]
    public string? VendorType { get; set; }

    [JsonPropertyName("Blocked")]
    public string? Blocked { get; set; }

    [JsonPropertyName("Privacy_Blocked")]
    public bool PrivacyBlocked { get; set; }

    [JsonPropertyName("Last_Date_Modified")]
    public string? LastDateModified { get; set; }
}
