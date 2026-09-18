namespace BcProxy.Models;

/// <summary>
/// Clean client DTO representing an Examination Sitting Cycle from Business Central ERP.
/// </summary>
public class ExaminationSittingCycleDto
{
    /// <summary>Exam sitting name/cycle (e.g. "APRIL 2024", "DECEMBER 2022")</summary>
    public string ExamSittingCycle { get; set; } = string.Empty;

    public string ExaminationProjectId { get; set; } = string.Empty;

    public string ProjectDescription { get; set; } = string.Empty;

    public string ExamStartDate { get; set; } = string.Empty;

    public string ExamEndDate { get; set; } = string.Empty;

    public bool Closed { get; set; }

    public string SittingStatus { get; set; } = string.Empty;

    public int SittingSequence { get; set; }

    public string DeferedSitting { get; set; } = string.Empty;
}
