using SuperUnit.Tests.Results.MethodResult;

namespace SuperUnit.Tests.Results.ExpectedResult;

public interface ITestExpectedResult
{
    public bool Test(IMethodResult actual);
}