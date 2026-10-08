using AcxiomCRM.Web.Data;
using AcxiomCRM.Web.Models;
using AcxiomCRM.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Web.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class CrmApiController:ControllerBase
{
    readonly ApplicationDbContext db; readonly UserManager<ApplicationUser> um; readonly AuditService audit;
    public CrmApiController(ApplicationDbContext d,UserManager<ApplicationUser> u,AuditService a){db=d;um=u;audit=a;}
    string U=>um.GetUserId(User)!; bool Priv=>User.IsInRole("Admin")||User.IsInRole("Manager");

    [HttpGet("customers")]
    public async Task<IActionResult> Customers([FromQuery]string? q){var x=db.Customers.AsQueryable();if(!Priv)x=x.Where(c=>c.OwnerId==U);if(!string.IsNullOrWhiteSpace(q))x=x.Where(c=>c.CustomerName.Contains(q)||c.Email.Contains(q)||c.CompanyName.Contains(q));return Ok(await x.Select(c=>new CustomerDto(c.Id,c.CustomerCode,c.CustomerName,c.Email,c.Phone,c.CompanyName,c.Status,c.OwnerId,c.CreatedAt)).ToListAsync());}

    [HttpGet("customers/{id}")]
    public async Task<IActionResult> Customer(int id){var c=await db.Customers.FindAsync(id);if(c==null)return NotFound(new{message="Customer not found"});if(!Priv&&c.OwnerId!=U)return Forbid();return Ok(new CustomerDto(c.Id,c.CustomerCode,c.CustomerName,c.Email,c.Phone,c.CompanyName,c.Status,c.OwnerId,c.CreatedAt));}

    [HttpPost("customers")]
    public async Task<IActionResult> CreateCustomer(CustomerWriteDto dto){if(!ModelState.IsValid)return ValidationProblem(ModelState);if(await db.Customers.AnyAsync(c=>c.Email==dto.Email||c.Phone==dto.Phone))return Conflict(new{message="Customer email or phone already exists."});var c=new Customer{CustomerCode="CUS-"+Random.Shared.Next(100000,999999),CustomerName=dto.CustomerName.Trim(),Email=dto.Email.Trim().ToLower(),Phone=dto.Phone,CompanyName=dto.CompanyName,Status=dto.Status,OwnerId=Priv&&dto.OwnerId!=null?dto.OwnerId:U};db.Customers.Add(c);await db.SaveChangesAsync();await audit.LogAsync("CREATE","Customer",c.Id.ToString(),null,c.CustomerName,U);return CreatedAtAction(nameof(Customer),new{id=c.Id},new CustomerDto(c.Id,c.CustomerCode,c.CustomerName,c.Email,c.Phone,c.CompanyName,c.Status,c.OwnerId,c.CreatedAt));}

    [HttpPut("customers/{id}")]
    public async Task<IActionResult> UpdateCustomer(int id,CustomerWriteDto dto){if(!ModelState.IsValid)return ValidationProblem(ModelState);var c=await db.Customers.FindAsync(id);if(c==null)return NotFound();if(!Priv&&c.OwnerId!=U)return Forbid();if(await db.Customers.AnyAsync(x=>x.Id!=id&&(x.Email==dto.Email||x.Phone==dto.Phone)))return Conflict(new{message="Customer email or phone already exists."});var old=$"{c.CustomerName}|{c.Email}|{c.Phone}|{c.Status}";c.CustomerName=dto.CustomerName.Trim();c.Email=dto.Email.Trim().ToLower();c.Phone=dto.Phone;c.CompanyName=dto.CompanyName;c.Status=dto.Status;c.ModifiedAt=DateTime.UtcNow;if(Priv&&dto.OwnerId!=null)c.OwnerId=dto.OwnerId;await db.SaveChangesAsync();await audit.LogAsync("UPDATE","Customer",id.ToString(),old,$"{c.CustomerName}|{c.Email}|{c.Phone}|{c.Status}",U);return Ok(new CustomerDto(c.Id,c.CustomerCode,c.CustomerName,c.Email,c.Phone,c.CompanyName,c.Status,c.OwnerId,c.CreatedAt));}

    [HttpDelete("customers/{id}")]
    public async Task<IActionResult> DeleteCustomer(int id){var c=await db.Customers.FindAsync(id);if(c==null)return NotFound();if(!Priv&&c.OwnerId!=U)return Forbid();c.Status=CustomerStatus.Inactive;c.ModifiedAt=DateTime.UtcNow;await db.SaveChangesAsync();await audit.LogAsync("DEACTIVATE","Customer",id.ToString(),null,"Inactive",U);return NoContent();}

    [HttpGet("leads")]
    public async Task<IActionResult> Leads(){var q=db.Leads.AsQueryable();if(!Priv)q=q.Where(x=>x.AssignedToId==U);return Ok(await q.Select(l=>new{l.Id,l.LeadCode,l.LeadName,l.Email,l.Phone,l.CompanyName,l.Source,l.Status,l.Priority,l.ExpectedValue,l.AssignedToId}).ToListAsync());}

    [HttpPost("leads")]
    public async Task<IActionResult> CreateLead(LeadWriteDto dto){if(!ModelState.IsValid)return ValidationProblem(ModelState);var l=new Lead{LeadCode="LED-"+Random.Shared.Next(100000,999999),LeadName=dto.LeadName,Email=dto.Email,Phone=dto.Phone,CompanyName=dto.CompanyName,Source=dto.Source,Status=dto.Status,Priority=dto.Priority,ExpectedValue=dto.ExpectedValue,AssignedToId=Priv&&dto.AssignedToId!=null?dto.AssignedToId:U};db.Leads.Add(l);await db.SaveChangesAsync();await audit.LogAsync("CREATE","Lead",l.Id.ToString(),null,l.LeadName,U);return Created($"/api/v1/leads/{l.Id}",new{l.Id,l.LeadCode,l.LeadName,l.Email,l.Phone,l.CompanyName,l.Source,l.Status,l.Priority,l.ExpectedValue,l.AssignedToId});}

    [HttpGet("opportunities")]
    public async Task<IActionResult> Opportunities(){var q=db.Opportunities.Include(o=>o.Customer).AsQueryable();if(!Priv)q=q.Where(x=>x.OwnerId==U);return Ok(await q.Select(o=>new{o.Id,o.OpportunityName,o.CustomerId,Customer=o.Customer!.CustomerName,o.Amount,o.Probability,o.Stage,o.Status,o.ExpectedCloseDate,o.OwnerId,WeightedValue=o.Amount*o.Probability/100m}).ToListAsync());}

    [HttpPost("opportunities")]
    public async Task<IActionResult> CreateOpportunity(OpportunityWriteDto dto){if(!ModelState.IsValid)return ValidationProblem(ModelState);if(dto.Status==OpportunityStatus.Open&&dto.Amount<=0)return BadRequest(new{message="Opportunity Amount must be greater than 0."});if(dto.Status==OpportunityStatus.Open&&dto.ExpectedCloseDate.Date<DateTime.UtcNow.Date)return BadRequest(new{message="Expected Close Date cannot be in the past."});if(!await db.Customers.AnyAsync(c=>c.Id==dto.CustomerId))return BadRequest(new{message="Customer does not exist."});var o=new Opportunity{OpportunityName=dto.OpportunityName,CustomerId=dto.CustomerId,LeadId=dto.LeadId,Amount=dto.Amount,Probability=dto.Probability,Stage=dto.Stage,Status=dto.Status,ExpectedCloseDate=dto.ExpectedCloseDate.Date,OwnerId=Priv&&dto.OwnerId!=null?dto.OwnerId:U,Notes=dto.Notes};db.Opportunities.Add(o);await db.SaveChangesAsync();await audit.LogAsync("CREATE","Opportunity",o.Id.ToString(),null,o.OpportunityName,U);return Created($"/api/v1/opportunities/{o.Id}",new{id=o.Id,o.OpportunityName,o.Amount,o.Probability,o.Stage,o.Status,o.ExpectedCloseDate,WeightedValue=o.WeightedValue});}
    [HttpGet("opportunities/{id}/risk")]
    public async Task<IActionResult> Risk(int id,[FromServices]RiskEngine engine){var o=await db.Opportunities.FindAsync(id);if(o==null)return NotFound();if(!Priv&&o.OwnerId!=U)return Forbid();var r=await engine.AnalyzeAsync(id);return Ok(new{r.OpportunityId,r.RiskScore,r.PredictedWinProbability,r.RiskLevel,r.FactorsJson,r.RecommendationsJson,r.GeneratedAt});}

}
