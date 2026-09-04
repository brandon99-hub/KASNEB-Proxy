using System.Text.Json.Serialization;

namespace BcProxy.Models;

/// <summary>
/// Maps to a row in the Business Central StudentsExamBookings OData entity.
/// </summary>
public class StudentExamBookingRecord
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

    [JsonPropertyName("Created_By")]
    public string? CreatedBy { get; set; }

    [JsonPropertyName("Created_On")]
    public string? CreatedOn { get; set; }

    [JsonPropertyName("Examination_Sitting")]
    public string? ExaminationSitting { get; set; }

    [JsonPropertyName("Booking_Receipt_No")]
    public string? BookingReceiptNo { get; set; }

    [JsonPropertyName("Booking_Invoice_No")]
    public string? BookingInvoiceNo { get; set; }
}
