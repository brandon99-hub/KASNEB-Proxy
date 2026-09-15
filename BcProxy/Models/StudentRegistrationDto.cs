namespace BcProxy.Models;

/// <summary>
/// Clean client DTO representing a student registration record from Business Central ERP.
/// </summary>
public class StudentRegistrationDto
{
    /// <summary>Registration document / application No (e.g. "10023" or "AP0001")</summary>
    public string No { get; set; } = string.Empty;

    /// <summary>Student customer number in Business Central (e.g. "ST00545448")</summary>
    public string StudentNo { get; set; } = string.Empty;

    public string StudentName { get; set; } = string.Empty;

    public string IdNumberPassportNo { get; set; } = string.Empty;

    public string ExaminationId { get; set; } = string.Empty;

    public string ExaminationDescription { get; set; } = string.Empty;

    public string Gender { get; set; } = string.Empty;

    public string DateOfBirth { get; set; } = string.Empty;

    public bool Disabled { get; set; }

    public string NcpwdNo { get; set; } = string.Empty;

    public string RegistrationDate { get; set; } = string.Empty;

    public string CreatedOn { get; set; } = string.Empty;

    public bool EmailSent { get; set; }

    /// <summary>Approval status of the registration (e.g. "Open", "Rejected")</summary>
    public string ApprovalStatus { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string HighestAcademicQualification { get; set; } = string.Empty;

    public string HighestAcademicQCode { get; set; } = string.Empty;
}
