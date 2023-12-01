using SuperUnit.Tests.Results.MethodResult;
using Xunit;
using Xunit.Sdk;

namespace SuperUnit.Tests.Results.ExpectedResult.Return;

public class EquivalenceTestExpectedResult : ITestExpectedResult
{
    public object ExpectedResult { get; }

    public EquivalenceTestExpectedResult(object expectedResult)
    {
        ExpectedResult = expectedResult;
    }

    public bool Test(IMethodResult actual)
    {
        if (actual is not ReturnValueMethodResult returnValueMethodResult)
            return false;

        try
        {
            Assert.Equivalent(ExpectedResult, returnValueMethodResult.ReturnValue);
        }
        catch (EquivalentException)
        {
            return false;
        }
        
        return true;
    }

    public override string ToString()
    {
        return $"equivalent of {ExpectedResult.ToTypedString()} returned";
    }
}