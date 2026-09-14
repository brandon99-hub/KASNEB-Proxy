using System.Text.Json.Serialization;

namespace BcProxy.Models;

/// <summary>
/// Maps directly to a row in the Business Central Studentregistrations OData entity.
/// Represents student registration records / applications from the ERP.
/// </summary>
public class StudentRegistrationRecord
{
    [JsonPropertyName("No")]
    public string? No { get; set; }

    [JsonPropertyName("Student_No")]
    public string? StudentNo { get; set; }

    [JsonPropertyName("Student_Name")]
    public string? StudentName { get; set; }

    [JsonPropertyName("ID_Number_Passport_No")]
    public string? IdNumberPassportNo { get; set; }

    [JsonPropertyName("Examination_ID")]
    public string? ExaminationId { get; set; }

    [JsonPropertyName("Examination_Description")]
    public string? ExaminationDescription { get; set; }

    [JsonPropertyName("Gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("Date_of_Birth")]
    public string? DateOfBirth { get; set; }

    [JsonPropertyName("Disabled")]
    public bool Disabled { get; set; }

    [JsonPropertyName("NCPWD_No")]
    public string? NcpwdNo { get; set; }

    [JsonPropertyName("Registration_Date")]
    public string? RegistrationDate { get; set; }

    [JsonPropertyName("Created_By")]
    public string? CreatedBy { get; set; }

    [JsonPropertyName("Created_On")]
    public string? CreatedOn { get; set; }

    [JsonPropertyName("Email_Sent")]
    public bool EmailSent { get; set; }

    [JsonPropertyName("Approval_Status")]
    public string? ApprovalStatus { get; set; }

    [JsonPropertyName("Email")]
    public string? Email { get; set; }

    [JsonPropertyName("Posted_On")]
    public string? PostedOn { get; set; }

    [JsonPropertyName("Posted_By")]
    public string? PostedBy { get; set; }

    [JsonPropertyName("Manual_Input")]
    public bool ManualInput { get; set; }

    [JsonPropertyName("Highest_Academic_Qualification")]
    public string? HighestAcademicQualification { get; set; }

    [JsonPropertyName("Highest_Academic_QCode")]
    public string? HighestAcademicQCode { get; set; }
}
