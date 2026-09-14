namespace BcProxy.Models;

/// <summary>
/// Clean DTO representing an Exam Region in Business Central.
/// </summary>
public class ExamRegionDto
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int NoOfZones { get; set; }
    public int NoOfExamCenters { get; set; }
    public bool Blocked { get; set; }
}
