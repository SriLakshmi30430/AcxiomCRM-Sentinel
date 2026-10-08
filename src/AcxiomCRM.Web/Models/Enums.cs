namespace AcxiomCRM.Web.Models;

public enum CustomerStatus { Active, Inactive, Prospect }
public enum LeadStatus { New, Contacted, Qualified, Unqualified, Converted, Lost }
public enum LeadPriority { Low, Medium, High, Critical }
public enum OpportunityStage { Qualification, Proposal, Negotiation, Won, Lost }
public enum OpportunityStatus { Open, Won, Lost }
public enum FollowUpStatus { Planned, Completed, Missed, Cancelled }
public enum FollowUpType { Call, Meeting, Email, Task }
public enum ActivityType { Call, Meeting, Email, Task }
public enum ActivityStatus { Planned, Completed, Cancelled }
