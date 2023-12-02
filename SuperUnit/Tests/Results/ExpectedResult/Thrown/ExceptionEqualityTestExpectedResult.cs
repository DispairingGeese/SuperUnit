using SuperUnit.Tests.Results.MethodResult;
using Xunit;
using Xunit.Sdk;

namespace SuperUnit.Tests.Results.ExpectedResult.Thrown;

public class ExceptionEqualityTestExpectedResult : ITestExpectedResult
{
    public object ExpectedExceptionThrown { get; }

    public ExceptionEqualityTestExpectedResult(object expectedExceptionThrown)
    {
        ExpectedExceptionThrown = expectedExceptionThrown;
    }
    
    public bool Test(IMethodResult actual)
    {
        if (actual is not ExceptionThrownMethodResult exceptionThrownMethodResult)
            return false;

        try
        {
            Assert.Equal(ExpectedExceptionThrown, exceptionThrownMethodResult.ThrownException);
        }
        catch (EqualException)
        {
            return false;
        }
        
        return true;
    }
    
    public override string ToString()
    {
        return $"exactly {ExpectedExceptionThrown.ToTypedString()} thrown";
    }
}
