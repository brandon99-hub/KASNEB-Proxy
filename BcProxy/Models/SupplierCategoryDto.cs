namespace BcProxy.Models;

/// <summary>
/// Clean DTO representing a Supplier Category.
/// </summary>
public class SupplierCategoryDto
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int NoPrequalified { get; set; }
}
