namespace BcProxy.Models;

/// <summary>
/// Clean DTO representing a Training Institution from Business Central.
/// </summary>
public class InstitutionDto
{
    /// <summary>Institution / Customer Code (e.g. "CUST00004")</summary>
    public string No { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string Address2 { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Contact { get; set; } = string.Empty;

    public string PhoneNo { get; set; } = string.Empty;

    public string Gender { get; set; } = string.Empty;

    public string AccreditationStatus { get; set; } = string.Empty;

    public string DateOfBirth { get; set; } = string.Empty;

    public bool Disabled { get; set; }

    public string AccreditationStartDate { get; set; } = string.Empty;

    public string AccreditationEndDate { get; set; } = string.Empty;

    public string NcpwdNo { get; set; } = string.Empty;
}
