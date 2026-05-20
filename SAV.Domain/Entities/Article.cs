using SAV.Domain.Common;

namespace SAV.Domain.Entities;

/// <summary>
/// Article entity representing a product or spare part.
/// </summary>
public class Article : BaseAuditableEntity
{
    /// <summary>
    /// Article/Product name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Serial number of the article.
    /// </summary>
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>
    /// Article sale price in Tunisian dinars.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Available stock quantity.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Image filename stored in wwwroot/images/.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Foreign key to Category.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// <summary>
    /// Navigation property to Category.
    /// </summary>
    public Category Category { get; set; } = null!;

    /// <summary>
    /// Related complaints for this article.
    /// </summary>
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}
