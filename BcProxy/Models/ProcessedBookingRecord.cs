using System.Text.Json.Serialization;

namespace BcProxy.Models;

/// <summary>
/// Maps to a row in the Business Central ProcessedBookingsCard OData entity.
/// Captures complete student exam venue allocation, financial references, and sitting project details.
/// </summary>
public class ProcessedBookingRecord
{
    [JsonPropertyName("No")]
    public string? No { get; set; }

    [JsonPropertyName("Student_No")]
    public string? StudentNo { get; set; }

    [JsonPropertyName("Student_Reg_No")]
    public string? StudentRegNo { get; set; }

    [JsonPropertyName("Student_Name")]
    public string? StudentName { get; set; }

    [JsonPropertyName("ID_Number_Passport_No")]
    public string? IdNumberPassportNo { get; set; }

    [JsonPropertyName("Gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("Date_of_Birth")]
    public string? DateOfBirth { get; set; }

    [JsonPropertyName("Disabled")]
    public bool Disabled { get; set; }

    [JsonPropertyName("Highest_Academic_QCode")]
    public string? HighestAcademicQCode { get; set; }

    [JsonPropertyName("Highest_Academic_Qualification")]
    public string? HighestAcademicQualification { get; set; }

    [JsonPropertyName("Examination_ID")]
    public string? ExaminationId { get; set; }

    [JsonPropertyName("Examination_Description")]
    public string? ExaminationDescription { get; set; }

    [JsonPropertyName("Examination_Center_Code")]
    public string? ExaminationCenterCode { get; set; }

    [JsonPropertyName("Examination_Center")]
    public string? ExaminationCenter { get; set; }

    [JsonPropertyName("Examination_Project_Code")]
    public string? ExaminationProjectCode { get; set; }

    [JsonPropertyName("Examination_Project_Name")]
    public string? ExaminationProjectName { get; set; }

    [JsonPropertyName("Examination_Sitting")]
    public string? ExaminationSitting { get; set; }

    [JsonPropertyName("Payment_Reference_No")]
    public string? PaymentReferenceNo { get; set; }

    [JsonPropertyName("Fee_Type")]
    public string? FeeType { get; set; }

    [JsonPropertyName("Created_By")]
    public string? CreatedBy { get; set; }

    [JsonPropertyName("Currency_Code")]
    public string? CurrencyCode { get; set; }

    [JsonPropertyName("Reason_for_Rejection")]
    public string? ReasonForRejection { get; set; }

    [JsonPropertyName("Institution_Code")]
    public string? InstitutionCode { get; set; }

    [JsonPropertyName("Institution_Name")]
    public string? InstitutionName { get; set; }

    [JsonPropertyName("Institution_Reference_No")]
    public string? InstitutionReferenceNo { get; set; }

    [JsonPropertyName("Booking_Amount")]
    public decimal BookingAmount { get; set; }

    [JsonPropertyName("Booking_Receipt_No")]
    public string? BookingReceiptNo { get; set; }

    [JsonPropertyName("Booking_Invoice_No")]
    public string? BookingInvoiceNo { get; set; }

    // Optional legacy fields for backward compatibility
    [JsonPropertyName("Date")]
    public string? Date { get; set; }

    [JsonPropertyName("Phone_No")]
    public string? PhoneNo { get; set; }

    [JsonPropertyName("Created_On")]
    public string? CreatedOn { get; set; }

    [JsonPropertyName("Posted_By")]
    public string? PostedBy { get; set; }

    [JsonPropertyName("Posted_On")]
    public string? PostedOn { get; set; }
}
