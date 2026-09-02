using System.Text.Json.Serialization;

namespace BcProxy.Models;

/// <summary>
/// Maps to a row in the Business Central SuppliersCategories OData entity.
/// </summary>
public class SupplierCategoryRecord
{
    [JsonPropertyName("Category_Code")]
    public string? CategoryCode { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("No_Prequalified")]
    public int? NoPrequalified { get; set; }

    [JsonPropertyName("Vendor_Posting_Group")]
    public string? VendorPostingGroup { get; set; }

    [JsonPropertyName("Gen_Bus_Posting_Group")]
    public string? GenBusPostingGroup { get; set; }

    [JsonPropertyName("VAT_Bus_Posting_Group")]
    public string? VatBusPostingGroup { get; set; }
}
