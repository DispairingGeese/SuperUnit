using SuperUnit.Tests.Results.ExpectedResult;
using SuperUnit.Tests.Results.MethodResult;

namespace SuperUnit.Tests.Results;

public readonly struct TestCaseResult
{
    public object[] Parameters { get; }
    
    public ITestExpectedResult ExpectedResult { get; }
    
    public IMethodResult MethodResult { get; }

    public bool IsSuccessful => ExpectedResult.Test(MethodResult);
    
    public TestCaseResult(object[] parameters, ITestExpectedResult expectedResult, IMethodResult methodResult)
    {
        Parameters = parameters;

        ExpectedResult = expectedResult;
        MethodResult = methodResult;
    }

    public override string ToString()
    {
        return $"Expected: {ExpectedResult}, Got: {MethodResult}";
    }
}