using AcxiomCRM.Web.Data;
using AcxiomCRM.Web.Models;
using Microsoft.AspNetCore.Http;

namespace AcxiomCRM.Web.Services;

public class AuditService
{
    private readonly ApplicationDbContext _db; private readonly IHttpContextAccessor _http;
    public AuditService(ApplicationDbContext db, IHttpContextAccessor http) { _db=db; _http=http; }
    public async Task LogAsync(string action,string entity,string record,string? oldValue,string? newValue,string? userId,string result="Success")
    {
        _db.AuditLogs.Add(new AuditLog{Action=action,EntityName=entity,RecordId=record,OldValue=oldValue,NewValue=newValue,UserId=userId,Result=result,IpAddress=_http.HttpContext?.Connection.RemoteIpAddress?.ToString(),CreatedAt=DateTime.UtcNow});
        await _db.SaveChangesAsync();
    }
}
