using SuperUnit.Tests.Results.ExpectedResult;
using SuperUnit.Tests.Results.MethodResult;

namespace SuperUnit.Tests;

public readonly struct TestCase
{
    public Func<object>? TargetActivator { get; }
    
    public object[] Parameters { get; }
    
    public ITestExpectedResult ExpectedResult { get; }

    public TestCase(Func<object>? targetActivator, object[] parameters, ITestExpectedResult expectedResult)
    {
        TargetActivator = targetActivator;
        
        Parameters = parameters;
        ExpectedResult = expectedResult;
    }
}