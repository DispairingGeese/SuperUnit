using SuperUnit.Tests.Results.MethodResult;
using Xunit;
using Xunit.Sdk;

namespace SuperUnit.Tests.Results.ExpectedResult.Thrown;

public class ExceptionTypeMatchExpectedResult : ITestExpectedResult
{
    public Type ExpectedTypeThrown { get; }

    public ExceptionTypeMatchExpectedResult(Type expectedTypeThrown)
    {
        ExpectedTypeThrown = expectedTypeThrown;
    }
    
    public bool Test(IMethodResult actual)
    {
        if (actual is not ExceptionThrownMethodResult exceptionThrownMethodResult)
            return false;

        try
        {
            Assert.Equal(ExpectedTypeThrown, exceptionThrownMethodResult.ThrownException.GetType());
        }
        catch (EqualException)
        {
            return false;
        }
        
        return true;
    }
    
    public override string ToString()
    {
        return $"exception of type {ExpectedTypeThrown.Name} thrown";
    }
}