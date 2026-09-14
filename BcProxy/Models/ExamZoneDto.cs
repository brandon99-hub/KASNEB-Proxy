namespace BcProxy.Models;

/// <summary>
/// Clean DTO representing an Exam Zone in Business Central.
/// </summary>
public class ExamZoneDto
{
    public string Code { get; set; } = string.Empty;
    public string ZoneName { get; set; } = string.Empty;
    public string RegionCode { get; set; } = string.Empty;
    public int NoOfExamCenters { get; set; }
}
