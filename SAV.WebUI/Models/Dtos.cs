namespace SAV.WebUI.Models;

// Auth DTOs
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}

public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

// Complaint DTOs
public class ComplaintDto
{
    public int Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public int ClientId { get; set; }
    public string? ClientName { get; set; }
}

public class CreateComplaintRequest
{
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ClientId { get; set; }
}

// Intervention DTOs
public class InterventionDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public int ComplaintId { get; set; }
    public int TechnicianId { get; set; }
    public string? TechnicianName { get; set; }
}

public class CreateInterventionRequest
{
    public string Type { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public int ComplaintId { get; set; }
    public int TechnicianId { get; set; }
}

// Invoice DTOs
public class InvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime IssuedDate { get; set; }
    public int ClientId { get; set; }
    public string? ClientName { get; set; }
}

// Dashboard DTOs
public class DashboardStats
{
    public int TotalComplaints { get; set; }
    public int PendingComplaints { get; set; }
    public int CompletedComplaints { get; set; }
    public int TotalInterventions { get; set; }
    public decimal TotalInvoiceAmount { get; set; }
}

// Client DTOs
public class ClientDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

// Technician DTOs
public class TechnicianDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
}
