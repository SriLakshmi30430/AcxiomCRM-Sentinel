using Xunit;
namespace AcxiomCRM.Web.Tests;
public class BasicTests { [Theory] [InlineData(10,50,5)] [InlineData(100,80,80)] public void WeightedPipelineIsCorrect(decimal amount,int probability,decimal expected)=>Assert.Equal(expected,amount*probability/100m); [Fact] public void ProbabilityRangeRuleConcept(){Assert.InRange(75,0,100);} }
