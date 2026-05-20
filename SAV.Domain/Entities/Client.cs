using SAV.Domain.Common;

namespace SAV.Domain.Entities;

/// <summary>
/// Client entity representing a customer in the system.
/// </summary>
public class Client : BaseAuditableEntity
{
    /// <summary>
    /// Client address.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Client phone number.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to User.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Navigation property to User.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Client complaints.
    /// </summary>
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}
