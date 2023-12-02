using SuperUnit.Tests.Results.MethodResult;
using Xunit;
using Xunit.Sdk;

namespace SuperUnit.Tests.Results.ExpectedResult.Thrown;

public class ExceptionEquivalenceTestExpectedResult : ITestExpectedResult
{
    public object ExpectedExceptionThrown { get; }

    public ExceptionEquivalenceTestExpectedResult(object expectedExceptionThrown)
    {
        ExpectedExceptionThrown = expectedExceptionThrown;
    }
    
    public bool Test(IMethodResult actual)
    {
        if (actual is not ExceptionThrownMethodResult exceptionThrownMethodResult)
            return false;

        try
        {
            Assert.Equivalent(ExpectedExceptionThrown, exceptionThrownMethodResult.ThrownException);
        }
        catch (EquivalentException)
        {
            return false;
        }
        
        return true;
    }
    
    public override string ToString()
    {
        return $"equivalent to {ExpectedExceptionThrown.ToTypedString()} thrown";
    }
}