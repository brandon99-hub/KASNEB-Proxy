using System.Text.Json.Serialization;

namespace BcProxy.Models;

/// <summary>
/// Maps to a row in the Business Central ExamResults OData entity.
/// </summary>
public class ExamResultRecord
{
    [JsonPropertyName("Line_No")]
    public int LineNo { get; set; }

    [JsonPropertyName("Examination")]
    public string? Examination { get; set; }

    [JsonPropertyName("Part")]
    public string? Part { get; set; }

    [JsonPropertyName("Section")]
    public string? Section { get; set; }

    [JsonPropertyName("Paper")]
    public string? Paper { get; set; }

    [JsonPropertyName("Paper_Name")]
    public string? PaperName { get; set; }

    [JsonPropertyName("Financial_Year")]
    public string? FinancialYear { get; set; }

    [JsonPropertyName("Grade")]
    public string? Grade { get; set; }

    [JsonPropertyName("Section_Grade")]
    public string? SectionGrade { get; set; }

    [JsonPropertyName("Section_Description")]
    public string? SectionDescription { get; set; }

    [JsonPropertyName("Examination_Sitting_ID")]
    public string? ExaminationSittingId { get; set; }

    [JsonPropertyName("Examination_Center")]
    public string? ExaminationCenter { get; set; }

    [JsonPropertyName("Mark")]
    public decimal Mark { get; set; }

    [JsonPropertyName("Passed")]
    public bool Passed { get; set; }

    [JsonPropertyName("Remarks")]
    public string? Remarks { get; set; }

    [JsonPropertyName("Student_Reg_No")]
    public string? StudentRegNo { get; set; }

    [JsonPropertyName("Registration_No")]
    public string? RegistrationNo { get; set; }

    [JsonPropertyName("Student_No")]
    public string? StudentNo { get; set; }
}
