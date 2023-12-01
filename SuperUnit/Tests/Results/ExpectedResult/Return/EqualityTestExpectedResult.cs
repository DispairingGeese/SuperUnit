using SuperUnit.Tests.Results.MethodResult;
using Xunit;
using Xunit.Sdk;

namespace SuperUnit.Tests.Results.ExpectedResult.Return;

public class EqualityTestExpectedResult : ITestExpectedResult
{
    public object ExpectedResult { get; }

    public EqualityTestExpectedResult(object expectedResult)
    {
        ExpectedResult = expectedResult;
    }

    public bool Test(IMethodResult actual)
    {
        if (actual is not ReturnValueMethodResult returnValueMethodResult)
            return false;

        try
        {
            Assert.Equal(ExpectedResult, returnValueMethodResult.ReturnValue);
        }
        catch (EqualException)
        {
            return false;
        }
        
        return true;
    }

    public override string ToString()
    {
        return $"exactly {ExpectedResult.ToTypedString()} returned";
    }
}