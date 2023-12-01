using SuperUnit.Tests.Results.MethodResult;
using Xunit;
using Xunit.Sdk;

namespace SuperUnit.Tests.Results.ExpectedResult.Thrown;

public class ExceptionThrownTestExpectedResult : ITestExpectedResult
{
    public object ExpectedExceptionThrown { get; }

    public ExceptionThrownTestExpectedResult(object expectedExceptionThrown)
    {
        ExpectedExceptionThrown = expectedExceptionThrown;
    }
    
    public bool Test(IMethodResult actual)
    {
        if (actual is not ExceptionThrownMethodResult returnValueMethodResult)
            return false;

        try
        {
            Assert.Equal(ExpectedExceptionThrown, returnValueMethodResult.ThrownException);
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
