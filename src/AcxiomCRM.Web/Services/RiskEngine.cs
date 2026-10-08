using System.Text.Json;
using AcxiomCRM.Web.Data;
using AcxiomCRM.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Web.Services;

// Local, dependency-free logistic risk model. It learns coefficients from historical won/lost opportunities.
public class RiskEngine
{
    private readonly ApplicationDbContext _db;
    public RiskEngine(ApplicationDbContext db) => _db=db;

    public async Task<OpportunityRiskScore> AnalyzeAsync(int opportunityId)
    {
        var opp=await _db.Opportunities.Include(o=>o.Customer).FirstOrDefaultAsync(o=>o.Id==opportunityId) ?? throw new KeyNotFoundException("Opportunity not found.");
        var history=await _db.Opportunities.Include(o=>o.Customer).Where(o=>o.Status!=OpportunityStatus.Open).ToListAsync();
        var features=await Features(opp);
        var weights=await TrainAsync(history);
        var z=weights.bias; for(int i=0;i<features.Length;i++) z+=weights.w[i]*features[i];
        var win=1.0/(1.0+Math.Exp(-z));
        var risk=Math.Round((1-win)*100,1);
        var factors=new List<object>();
        if(features[0]>0.65) factors.Add(new {factor="Opportunity inactivity / aging",impact=Math.Round(features[0]*25,1),severity="high"});
        if(features[1]>0.5) factors.Add(new {factor="Missed follow-ups",impact=Math.Round(features[1]*25,1),severity="high"});
        if(features[2]>0.65) factors.Add(new {factor="Close-date pressure",impact=Math.Round(features[2]*20,1),severity="medium"});
        if(features[3]<0.4) factors.Add(new {factor="Low current probability",impact=Math.Round((1-features[3])*20,1),severity="medium"});
        if(!factors.Any()) factors.Add(new {factor="No dominant risk factor detected",impact=5.0,severity="low"});
        var recs=new List<string>();
        if(risk>=70) recs.Add("Schedule a customer touchpoint within 24 hours.");
        if(features[1]>0.5) recs.Add("Reschedule or complete missed follow-ups and record the outcome.");
        if(features[2]>0.65) recs.Add("Ask the owner to confirm the expected close date.");
        if(features[3]<0.4) recs.Add("Review qualification evidence before keeping a high pipeline value.");
        if(!recs.Any()) recs.Add("Continue normal cadence and record the next customer interaction.");
        var entity=new OpportunityRiskScore{OpportunityId=opp.Id,RiskScore=risk,PredictedWinProbability=Math.Round(win*100,1),RiskLevel=risk>=80?"CRITICAL":risk>=60?"HIGH":risk>=35?"MEDIUM":"LOW",FactorsJson=JsonSerializer.Serialize(factors),RecommendationsJson=JsonSerializer.Serialize(recs)};
        _db.OpportunityRiskScores.Add(entity); await _db.SaveChangesAsync(); return entity;
    }

    private async Task<double[]> Features(Opportunity o)
    {
        var last=await _db.Activities.Where(a=>a.CustomerId==o.CustomerId).OrderByDescending(a=>a.ActivityDate).Select(a=>(DateTime?)a.ActivityDate).FirstOrDefaultAsync();
        var daysInactive=last.HasValue?Math.Max(0,(DateTime.UtcNow-last.Value).TotalDays):30;
        var missed=await _db.FollowUps.CountAsync(f=>f.OpportunityId==o.Id && f.Status==FollowUpStatus.Missed);
        var daysToClose=(o.ExpectedCloseDate-DateTime.UtcNow.Date).TotalDays;
        return new[]{Math.Min(1,daysInactive/30.0),Math.Min(1,missed/3.0),Math.Min(1,Math.Max(0,(7-daysToClose)/7.0)),o.Probability/100.0};
    }

    private async Task<(double[] w,double bias)> TrainAsync(List<Opportunity> history)
    {
        if(history.Count<6) return (new[]{-1.1,-1.0,-0.8,2.2},-0.8);
        var samples=new List<(double[] x,double y)>();
        foreach(var o in history) samples.Add((await Features(o),o.Status==OpportunityStatus.Won?1:0));
        var w=new double[]{-1,-1,-1,2}; var b=0.0; var lr=0.15;
        for(int epoch=0;epoch<300;epoch++) foreach(var sample in samples){
            var z=b; for(int i=0;i<4;i++)z+=w[i]*sample.x[i]; var p=1.0/(1.0+Math.Exp(-z)); var e=p-sample.y;
            for(int i=0;i<4;i++)w[i]-=lr*e*sample.x[i]; b-=lr*e;
        }
        return (w,b);
    }
}
