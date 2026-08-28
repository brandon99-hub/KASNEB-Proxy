using System.Text.Json.Serialization;

namespace BcProxy.Models;

/// <summary>
/// Maps directly to a row in the Business Central ExamAccounts OData entity.
/// Each row represents one KASNEB registration/exam account for a student.
/// A single student (identified by Student_Cust_No) may have multiple exam accounts
/// if they are enrolled in multiple courses.
/// </summary>
public class ExamAccount
{
    /// <summary>KASNEB registration number (e.g. "2021/CPA/03987" or "NAC/181912")</summary>
    [JsonPropertyName("Registration_No")]
    public string? RegistrationNo { get; set; }

    /// <summary>FK to Studentlist.No (e.g. "ST00540998")</summary>
    [JsonPropertyName("Student_Cust_No")]
    public string? StudentCustNo { get; set; }

    [JsonPropertyName("Registration_Date")]
    public string? RegistrationDate { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("First_Name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("Middle_Name")]
    public string? MiddleName { get; set; }

    [JsonPropertyName("Surname")]
    public string? Surname { get; set; }

    [JsonPropertyName("Balance")]
    public decimal Balance { get; set; }

    [JsonPropertyName("Course_ID")]
    public string? CourseId { get; set; }

    [JsonPropertyName("Course_Description")]
    public string? CourseDescription { get; set; }

    [JsonPropertyName("Blocked")]
    public bool Blocked { get; set; }

    [JsonPropertyName("Blocking_Remarks")]
    public string? BlockingRemarks { get; set; }

    [JsonPropertyName("Renewal_Amount")]
    public decimal RenewalAmount { get; set; }

    [JsonPropertyName("Re_Activation_Amount")]
    public decimal ReActivationAmount { get; set; }

    [JsonPropertyName("Renewal_Pending")]
    public int RenewalPending { get; set; }

    [JsonPropertyName("Status")]
    public string? Status { get; set; }

    [JsonPropertyName("Status_Remarks")]
    public string? StatusRemarks { get; set; }

    [JsonPropertyName("Email")]
    public string? Email { get; set; }

    [JsonPropertyName("Phone_No")]
    public string? PhoneNo { get; set; }

    [JsonPropertyName("ID_No")]
    public string? IdNo { get; set; }

    [JsonPropertyName("Kasneb_Foundation")]
    public bool KasnebFoundation { get; set; }

    [JsonPropertyName("Total_Amount_Rece_From_Helb")]
    public decimal TotalAmountFromHelb { get; set; }

    [JsonPropertyName("Last_Exam_Date")]
    public string? LastExamDate { get; set; }

    [JsonPropertyName("Last_Payment_Date")]
    public string? LastPaymentDate { get; set; }
}
