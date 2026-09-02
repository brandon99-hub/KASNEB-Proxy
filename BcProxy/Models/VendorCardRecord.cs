using System.Text.Json.Serialization;

namespace BcProxy.Models;

/// <summary>
/// Maps directly to a row in the Business Central VendorCard OData entity.
/// </summary>
public class VendorCardRecord
{
    [JsonPropertyName("No")]
    public string? No { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Search_Name")]
    public string? SearchName { get; set; }

    [JsonPropertyName("Responsibility_Center")]
    public string? ResponsibilityCenter { get; set; }

    [JsonPropertyName("Phone_No")]
    public string? PhoneNo { get; set; }

    [JsonPropertyName("E_Mail")]
    public string? EMail { get; set; }

    [JsonPropertyName("Fax_No")]
    public string? FaxNo { get; set; }

    [JsonPropertyName("Contact")]
    public string? Contact { get; set; }

    [JsonPropertyName("Vendor_Type")]
    public string? VendorType { get; set; }

    [JsonPropertyName("Supplier_Category")]
    public string? SupplierCategory { get; set; }

    [JsonPropertyName("Special_Category")]
    public string? SpecialCategory { get; set; }

    [JsonPropertyName("_x003C_AGPO_Cert_No__x003E_")]
    public string? AgpoCertNo { get; set; }

    [JsonPropertyName("Category")]
    public string? Category { get; set; }

    [JsonPropertyName("Supplier_Type")]
    public string? SupplierType { get; set; }

    [JsonPropertyName("Vendor_Group")]
    public string? VendorGroup { get; set; }

    [JsonPropertyName("Address")]
    public string? Address { get; set; }

    [JsonPropertyName("Address_2")]
    public string? Address2 { get; set; }

    [JsonPropertyName("City")]
    public string? City { get; set; }

    [JsonPropertyName("County")]
    public string? County { get; set; }

    [JsonPropertyName("Blocked")]
    public string? Blocked { get; set; }

    [JsonPropertyName("Privacy_Blocked")]
    public bool PrivacyBlocked { get; set; }

    [JsonPropertyName("Last_Date_Modified")]
    public string? LastDateModified { get; set; }
}
