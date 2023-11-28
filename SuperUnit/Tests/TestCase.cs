using SuperUnit.Tests.Results.MethodResult;

namespace SuperUnit.Tests;

public readonly struct TestCase
{
    public Func<object>? TargetActivator { get; }
    
    public object[] Parameters { get; }
    
    public IMethodResult ExpectedResult { get; }

    public TestCase(Func<object>? targetActivator, object[] parameters, IMethodResult expectedResult)
    {
        TargetActivator = targetActivator;
        
        Parameters = parameters;
        ExpectedResult = expectedResult;
    }
}