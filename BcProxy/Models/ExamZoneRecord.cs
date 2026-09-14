using System.Text.Json.Serialization;

namespace BcProxy.Models;

/// <summary>
/// Maps directly to a row in the Business Central ExamZones OData entity.
/// </summary>
public class ExamZoneRecord
{
    [JsonPropertyName("Code")]
    public string? Code { get; set; }

    [JsonPropertyName("Zone_Name")]
    public string? ZoneName { get; set; }

    [JsonPropertyName("Region_Code")]
    public string? RegionCode { get; set; }

    [JsonPropertyName("No_of_Exam_Centers")]
    public int? NoOfExamCenters { get; set; }
}
