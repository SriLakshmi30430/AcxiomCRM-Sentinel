using AcxiomCRM.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<FollowUp> FollowUps => Set<FollowUp>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<OpportunityRiskScore> OpportunityRiskScores => Set<OpportunityRiskScore>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<Customer>().HasIndex(x => x.Email).IsUnique();
        b.Entity<Customer>().HasIndex(x => x.Phone).IsUnique();
        b.Entity<Customer>().HasIndex(x => x.CustomerCode).IsUnique();
        b.Entity<Lead>().HasIndex(x => x.LeadCode).IsUnique();
        b.Entity<Opportunity>().Property(x => x.Amount).HasPrecision(18, 2);
        b.Entity<Opportunity>().HasOne(x => x.Customer).WithMany(x => x.Opportunities).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Opportunity>().HasOne(x => x.Lead).WithMany().HasForeignKey(x => x.LeadId).OnDelete(DeleteBehavior.SetNull);
        b.Entity<Lead>().HasOne(x => x.Customer).WithMany(x => x.Leads).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.SetNull);
        b.Entity<AuditLog>().Property(x => x.OldValue).HasMaxLength(4000);
        b.Entity<AuditLog>().Property(x => x.NewValue).HasMaxLength(4000);
        b.Entity<OpportunityRiskScore>().HasIndex(x => new { x.OpportunityId, x.GeneratedAt });
    }
}
