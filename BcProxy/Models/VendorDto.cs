namespace BcProxy.Models;

/// <summary>
/// Clean DTO representing a Vendor from Business Central.
/// Excludes posting and financial fields per specification.
/// </summary>
public class VendorDto
{
    /// <summary>Vendor Code (e.g. "00066")</summary>
    public string No { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string SearchName { get; set; } = string.Empty;

    public string Contact { get; set; } = string.Empty;

    public string PhoneNo { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string FaxNo { get; set; } = string.Empty;

    public string ResponsibilityCenter { get; set; } = string.Empty;

    public string LocationCode { get; set; } = string.Empty;

    public string PostCode { get; set; } = string.Empty;

    public string CountryRegionCode { get; set; } = string.Empty;

    public string VendorType { get; set; } = string.Empty;

    public string Blocked { get; set; } = string.Empty;

    public bool PrivacyBlocked { get; set; }

    public string LastDateModified { get; set; } = string.Empty;
}
