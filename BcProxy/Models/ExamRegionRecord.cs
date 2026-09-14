using System.Text.Json.Serialization;

namespace BcProxy.Models;

/// <summary>
/// Maps directly to a row in the Business Central ExamRegions OData entity.
/// </summary>
public class ExamRegionRecord
{
    [JsonPropertyName("Code")]
    public string? Code { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("No_of_Zones")]
    public int? NoOfZones { get; set; }

    [JsonPropertyName("No_of_Exam_Centers")]
    public int? NoOfExamCenters { get; set; }

    [JsonPropertyName("Blocked")]
    public bool Blocked { get; set; }
}
