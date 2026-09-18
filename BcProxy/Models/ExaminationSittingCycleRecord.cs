using System.Text.Json.Serialization;

namespace BcProxy.Models;

/// <summary>
/// Maps directly to a row in the Business Central ExaminationSittingCycle OData entity.
/// Defines master exam sitting cycles, project descriptions, dates, and closure statuses.
/// </summary>
public class ExaminationSittingCycleRecord
{
    [JsonPropertyName("Exam_Sitting_Cycle")]
    public string? ExamSittingCycle { get; set; }

    [JsonPropertyName("Examination_Project_ID")]
    public string? ExaminationProjectId { get; set; }

    [JsonPropertyName("Project_Description")]
    public string? ProjectDescription { get; set; }

    [JsonPropertyName("Exam_Start_Date")]
    public string? ExamStartDate { get; set; }

    [JsonPropertyName("Exam_End_Date")]
    public string? ExamEndDate { get; set; }

    [JsonPropertyName("Closed")]
    public bool Closed { get; set; }

    [JsonPropertyName("Sitting_Status")]
    public string? SittingStatus { get; set; }

    [JsonPropertyName("Sitting_Sequence")]
    public int? SittingSequence { get; set; }

    [JsonPropertyName("Defered_Sitting")]
    public string? DeferedSitting { get; set; }
}
