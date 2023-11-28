using SuperUnit.Tests.Results.MethodResult;

namespace SuperUnit.Tests.Results;

public readonly struct TestCaseResult
{
    public object[] Parameters { get; }
    
    public IMethodResult ExpectedResult { get; }
    
    public IMethodResult ActualResult { get; }

    public bool IsSuccessful => ExpectedResult.Matches(ActualResult);
    
    public TestCaseResult(object[] parameters, IMethodResult expectedResult, IMethodResult actualResult)
    {
        Parameters = parameters;
        
        ExpectedResult = expectedResult;
        ActualResult = actualResult;
    }

    public override string ToString()
    {
        return $"Expected: {ExpectedResult}, Got: {ActualResult}";
    }
}