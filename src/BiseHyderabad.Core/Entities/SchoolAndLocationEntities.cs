using BiseHyderabad.Core.Enums;

namespace BiseHyderabad.Core.Entities;

public class Tenant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

}

public class District
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty; // e.g. "HY", "KP", "NF"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Tehsil> Tehsils { get; set; } = new();
    public List<School> Schools { get; set; } = new();
    public List<User> Users { get; set; } = new();
}

public class Tehsil
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public int DistrictId { get; set; }
    public District? District { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<School> Schools { get; set; } = new();
}

public class School
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string SemisCode { get; set; } = string.Empty;
    public string? Code { get; set; } // Short code / sequential code e.g. "001"
    public string Name { get; set; } = string.Empty;
    public int DistrictId { get; set; }
    public District? District { get; set; }
    public int? TehsilId { get; set; }
    public Tehsil? Tehsil { get; set; }
    public SchoolType Type { get; set; } = SchoolType.Public;
    public int Zone { get; set; } = 1;
    public string? Address { get; set; }
    public string? ContactNumber { get; set; }
    public string? HeadName { get; set; }
    public string? HeadPhone { get; set; }
    public string? HeadCnic { get; set; }
    public string? HeadEmail { get; set; }
    public string AllowedLevelsJson { get; set; } = "[\"SSC-I\",\"SSC-II\",\"HSC-I\",\"HSC-II\"]";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<User> Users { get; set; } = new();
    public List<Enrollment> Enrollments { get; set; } = new();
    public List<Invoice> Invoices { get; set; } = new();
    public List<SchoolSpecialPermission> SpecialPermissions { get; set; } = new();
}

public class SchoolSpecialPermission
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public int SchoolId { get; set; }
    public School? School { get; set; }
    public string PermissionType { get; set; } = "extend_deadline";
    public string? Reason { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
