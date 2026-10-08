using AcxiomCRM.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<ApplicationDbContext>();
        foreach (var role in new[] { "Admin", "Manager", "SalesExecutive" })
            if (!await roleManager.RoleExistsAsync(role)) await roleManager.CreateAsync(new IdentityRole(role));

        await EnsureUser(userManager, "admin@acxiom.local", "Admin@123", "System Administrator", "Admin");
        var manager = await EnsureUser(userManager, "manager@acxiom.local", "Manager@123", "Sales Manager", "Manager");
        var sales = await EnsureUser(userManager, "sales@acxiom.local", "Sales@123", "Sales Executive", "SalesExecutive");

        if (!await db.Customers.AnyAsync())
        {
            db.Customers.AddRange(
                new Customer { CustomerCode="CUS-1001", CustomerName="Apex Manufacturing", Email="contact@apex.example", Phone="9876543210", CompanyName="Apex Manufacturing", City="Hyderabad", State="Telangana", OwnerId=manager.Id },
                new Customer { CustomerCode="CUS-1002", CustomerName="Nova Retail Group", Email="hello@nova.example", Phone="9876543211", CompanyName="Nova Retail Group", City="Bengaluru", State="Karnataka", OwnerId=sales.Id },
                new Customer { CustomerCode="CUS-1003", CustomerName="Vertex Finance", Email="ops@vertex.example", Phone="9876543212", CompanyName="Vertex Finance", City="Mumbai", State="Maharashtra", OwnerId=sales.Id });
            await db.SaveChangesAsync();
        }
        if (!await db.Leads.AnyAsync())
        {
            var c = await db.Customers.OrderBy(x=>x.Id).FirstAsync();
            db.Leads.AddRange(
                new Lead { LeadCode="LED-1001", LeadName="Priya Sharma", Email="priya@prospect.example", Phone="9876500001", CompanyName="Orbit Logistics", Source="Website", Status=LeadStatus.Qualified, Priority=LeadPriority.High, ExpectedValue=800000, AssignedToId=sales.Id },
                new Lead { LeadCode="LED-1002", LeadName="Arjun Mehta", Email="arjun@prospect.example", Phone="9876500002", CompanyName="Bright Energy", Source="Referral", Status=LeadStatus.Contacted, Priority=LeadPriority.Medium, ExpectedValue=450000, AssignedToId=sales.Id, CustomerId=c.Id });
            await db.SaveChangesAsync();
        }
        if (!await db.Opportunities.AnyAsync())
        {
            var customers = await db.Customers.OrderBy(x=>x.Id).Take(3).ToListAsync();
            for (int i=0;i<36;i++)
            {
                var stage=(OpportunityStage)(i%5);
                var won=stage==OpportunityStage.Won;
                var lost=stage==OpportunityStage.Lost;
                db.Opportunities.Add(new Opportunity { OpportunityName=$"Enterprise Deal {i+1:00}", CustomerId=customers[i%customers.Count].Id, Amount=150000 + i*27500, Probability=won?100:lost?10:35+(i%6)*10, Stage=stage, Status=won?OpportunityStatus.Won:lost?OpportunityStatus.Lost:OpportunityStatus.Open, ExpectedCloseDate=DateTime.UtcNow.Date.AddDays((i%12)-3), OwnerId=i%3==0?manager.Id:sales.Id, Notes="Seeded historical sales record for local intelligence training." });
            }
            await db.SaveChangesAsync();
        }
        if (!await db.FollowUps.AnyAsync())
        {
            var opps=await db.Opportunities.Take(10).ToListAsync();
            foreach(var o in opps) db.FollowUps.Add(new FollowUp{OpportunityId=o.Id, FollowUpDate=DateTime.UtcNow.Date.AddDays(o.Id%7-2), FollowUpType=FollowUpType.Call, Subject=$"Follow up: {o.OpportunityName}", Status=o.Id%4==0?FollowUpStatus.Missed:FollowUpStatus.Planned, AssignedToId=o.OwnerId});
            await db.SaveChangesAsync();
        }
    }

    private static async Task<ApplicationUser> EnsureUser(UserManager<ApplicationUser> um,string email,string password,string name,string role)
    {
        var user=await um.FindByEmailAsync(email);
        if(user is null){ user=new ApplicationUser{UserName=email,Email=email,EmailConfirmed=true,FullName=name,IsActive=true}; var result=await um.CreateAsync(user,password); if(!result.Succeeded) throw new Exception(string.Join("; ",result.Errors.Select(e=>e.Description))); }
        if(!await um.IsInRoleAsync(user,role)) await um.AddToRoleAsync(user,role);
        return user;
    }
}
