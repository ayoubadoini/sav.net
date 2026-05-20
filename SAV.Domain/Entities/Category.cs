using SAV.Domain.Common;

namespace SAV.Domain.Entities;

/// <summary>
/// Category entity representing a product category.
/// </summary>
public class Category : BaseAuditableEntity
{
    /// <summary>
    /// Category name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Articles in this category.
    /// </summary>
    public ICollection<Article> Articles { get; set; } = new List<Article>();
}
