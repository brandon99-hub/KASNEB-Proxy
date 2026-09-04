using System.Text.Json.Serialization;

namespace BcProxy.Models;

/// <summary>
/// Maps to a row in the Business Central ProcessedBookings OData entity.
/// </summary>
public class ProcessedBookingRecord
{
    [JsonPropertyName("No")]
    public string? No { get; set; }

    [JsonPropertyName("Date")]
    public string? Date { get; set; }

    [JsonPropertyName("Student_No")]
    public string? StudentNo { get; set; }

    [JsonPropertyName("Student_Name")]
    public string? StudentName { get; set; }

    [JsonPropertyName("Student_Reg_No")]
    public string? StudentRegNo { get; set; }

    [JsonPropertyName("ID_Number_Passport_No")]
    public string? IdNumberPassportNo { get; set; }

    [JsonPropertyName("Examination_ID")]
    public string? ExaminationId { get; set; }

    [JsonPropertyName("Examination_Description")]
    public string? ExaminationDescription { get; set; }

    [JsonPropertyName("Booking_Amount")]
    public decimal BookingAmount { get; set; }

    [JsonPropertyName("Examination_Center_Code")]
    public string? ExaminationCenterCode { get; set; }

    [JsonPropertyName("Phone_No")]
    public string? PhoneNo { get; set; }

    [JsonPropertyName("Examination_Center")]
    public string? ExaminationCenter { get; set; }

    [JsonPropertyName("Gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("Disabled")]
    public bool Disabled { get; set; }

    [JsonPropertyName("Created_By")]
    public string? CreatedBy { get; set; }

    [JsonPropertyName("Created_On")]
    public string? CreatedOn { get; set; }

    [JsonPropertyName("Posted_By")]
    public string? PostedBy { get; set; }

    [JsonPropertyName("Posted_On")]
    public string? PostedOn { get; set; }
}
