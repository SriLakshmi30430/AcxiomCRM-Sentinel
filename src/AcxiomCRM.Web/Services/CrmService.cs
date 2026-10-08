using AcxiomCRM.Web.Data;
using AcxiomCRM.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Web.Services;

public class CrmService
{
    private readonly ApplicationDbContext _db;
    public CrmService(ApplicationDbContext db) => _db=db;

    public IQueryable<Customer> CustomersFor(string? userId,bool privileged) => privileged ? _db.Customers.Include(x=>x.Owner) : _db.Customers.Include(x=>x.Owner).Where(x=>x.OwnerId==userId);
    public IQueryable<Lead> LeadsFor(string? userId,bool privileged) => privileged ? _db.Leads.Include(x=>x.AssignedTo).Include(x=>x.Customer) : _db.Leads.Include(x=>x.AssignedTo).Include(x=>x.Customer).Where(x=>x.AssignedToId==userId);
    public IQueryable<Opportunity> OpportunitiesFor(string? userId,bool privileged) => privileged ? _db.Opportunities.Include(x=>x.Customer).Include(x=>x.Owner) : _db.Opportunities.Include(x=>x.Customer).Include(x=>x.Owner).Where(x=>x.OwnerId==userId);
    public IQueryable<FollowUp> FollowUpsFor(string? userId,bool privileged) => privileged ? _db.FollowUps.Include(x=>x.Customer).Include(x=>x.Opportunity).Include(x=>x.AssignedTo) : _db.FollowUps.Include(x=>x.Customer).Include(x=>x.Opportunity).Include(x=>x.AssignedTo).Where(x=>x.AssignedToId==userId);
}
