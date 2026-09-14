namespace BcProxy.Models;

/// <summary>
/// Clean, enriched DTO representing an Exam Center from Business Central ERP.
/// Incorporates resolved geographic zone and region descriptions, and identifies
/// dual-purpose venues that operate as both training institutions and exam centers.
/// </summary>
public class ExamCenterDto
{
    /// <summary>Exam Center Code (e.g. "1001")</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Exam Center Name (e.g. "K.H.C ABUJA NIGERIA")</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Zone code foreign key (e.g. "6")</summary>
    public string ExamZone { get; set; } = string.Empty;

    /// <summary>Resolved Exam Zone name from ExamZones lookup (e.g. "MOMBASA")</summary>
    public string ZoneName { get; set; } = string.Empty;

    /// <summary>Region code foreign key (e.g. "49")</summary>
    public string ExamRegion { get; set; } = string.Empty;

    /// <summary>Resolved Exam Region description from ExamRegions lookup (e.g. "Outside Kenya")</summary>
    public string RegionName { get; set; } = string.Empty;

    public string ExamRoute { get; set; } = string.Empty;

    public bool DisabilityFriendly { get; set; }

    public bool ComputerBasedFriendly { get; set; }

    public bool KisebCenters { get; set; }

    public bool ExamBufferZone { get; set; }

    public bool NeutralBufferZone { get; set; }

    public bool Blocked { get; set; }

    /// <summary>Training institution identifier / code</summary>
    public string TrainingInstitution { get; set; } = string.Empty;

    /// <summary>Associated parent or affiliated institution</summary>
    public string Institution { get; set; } = string.Empty;

    /// <summary>True if this exam center is also recognized as a training institution</summary>
    public bool IsTrainingInstitution { get; set; }

    /// <summary>Center classification (e.g. "Foreign", "Local")</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Operational status (e.g. "Active")</summary>
    public string Status { get; set; } = string.Empty;

    public int MaximumCapacityPerSession { get; set; }

    public int TotalBookedStudents { get; set; }

    public string Address { get; set; } = string.Empty;

    public string Address2 { get; set; } = string.Empty;

    public string PostCode { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string CountryRegionCode { get; set; } = string.Empty;

    public string ShowMap { get; set; } = string.Empty;

    public string Contact { get; set; } = string.Empty;

    public string PhoneNo { get; set; } = string.Empty;

    public string FaxNo { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string HomePage { get; set; } = string.Empty;
}
