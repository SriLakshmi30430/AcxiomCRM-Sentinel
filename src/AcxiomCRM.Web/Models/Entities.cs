using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Web.Models;

public class ApplicationUser : IdentityUser
{
    [Required, MaxLength(100)] public string FullName { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Customer
{
    public int Id { get; set; }
    [Required, MaxLength(20)] public string CustomerCode { get; set; } = "";
    [Required, MaxLength(120)] public string CustomerName { get; set; } = "";
    [Required, EmailAddress, MaxLength(160)] public string Email { get; set; } = "";
    [Required, MaxLength(20)] public string Phone { get; set; } = "";
    [MaxLength(160)] public string CompanyName { get; set; } = "";
    [MaxLength(250)] public string Address { get; set; } = "";
    [MaxLength(80)] public string City { get; set; } = "";
    [MaxLength(80)] public string State { get; set; } = "";
    public CustomerStatus Status { get; set; } = CustomerStatus.Active;
    public string? OwnerId { get; set; }
    public ApplicationUser? Owner { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
}

public class Lead
{
    public int Id { get; set; }
    [Required, MaxLength(20)] public string LeadCode { get; set; } = "";
    [Required, MaxLength(120)] public string LeadName { get; set; } = "";
    [Required, EmailAddress, MaxLength(160)] public string Email { get; set; } = "";
    [Required, MaxLength(20)] public string Phone { get; set; } = "";
    [MaxLength(160)] public string CompanyName { get; set; } = "";
    [MaxLength(80)] public string Source { get; set; } = "Website";
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public LeadPriority Priority { get; set; } = LeadPriority.Medium;
    public decimal ExpectedValue { get; set; }
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string? AssignedToId { get; set; }
    public ApplicationUser? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Opportunity
{
    public int Id { get; set; }
    [Required, MaxLength(160)] public string OpportunityName { get; set; } = "";
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? LeadId { get; set; }
    public Lead? Lead { get; set; }
    [Range(0, double.MaxValue)] public decimal Amount { get; set; }
    [Range(0, 100)] public int Probability { get; set; }
    public OpportunityStage Stage { get; set; } = OpportunityStage.Qualification;
    public OpportunityStatus Status { get; set; } = OpportunityStatus.Open;
    public DateTime ExpectedCloseDate { get; set; } = DateTime.UtcNow.Date.AddDays(30);
    public string? OwnerId { get; set; }
    public ApplicationUser? Owner { get; set; }
    [MaxLength(1000)] public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public decimal WeightedValue => Amount * Probability / 100m;
}

public class FollowUp
{
    public int Id { get; set; }
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? LeadId { get; set; }
    public Lead? Lead { get; set; }
    public int? OpportunityId { get; set; }
    public Opportunity? Opportunity { get; set; }
    public DateTime FollowUpDate { get; set; }
    public FollowUpType FollowUpType { get; set; } = FollowUpType.Call;
    [Required, MaxLength(160)] public string Subject { get; set; } = "";
    [MaxLength(1000)] public string? Remarks { get; set; }
    public FollowUpStatus Status { get; set; } = FollowUpStatus.Planned;
    public string? AssignedToId { get; set; }
    public ApplicationUser? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Activity
{
    public int Id { get; set; }
    public ActivityType ActivityType { get; set; }
    [Required, MaxLength(160)] public string Subject { get; set; } = "";
    [MaxLength(1500)] public string? Description { get; set; }
    public DateTime ActivityDate { get; set; } = DateTime.UtcNow;
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int? LeadId { get; set; }
    public Lead? Lead { get; set; }
    public string? AssignedToId { get; set; }
    public ApplicationUser? AssignedTo { get; set; }
    public ActivityStatus Status { get; set; } = ActivityStatus.Completed;
}

public class AuditLog
{
    public long Id { get; set; }
    public string? UserId { get; set; }
    public string Action { get; set; } = "";
    public string EntityName { get; set; } = "";
    public string RecordId { get; set; } = "";
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string Result { get; set; } = "Success";
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class OpportunityRiskScore
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public Opportunity? Opportunity { get; set; }
    public double RiskScore { get; set; }
    public double PredictedWinProbability { get; set; }
    public string RiskLevel { get; set; } = "LOW";
    public string FactorsJson { get; set; } = "[]";
    public string RecommendationsJson { get; set; } = "[]";
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
