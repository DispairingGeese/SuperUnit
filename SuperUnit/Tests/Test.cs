using System.Reflection;
using SuperUnit.Tests.Results;
using SuperUnit.Tests.Results.MethodResult;

namespace SuperUnit.Tests;

public readonly struct Test
{
    private readonly MethodInfo _method;

    private readonly TestCase[] _testCases;
    
    public Test(MethodInfo method, TestCase[] testCases)
    {
        _method = method;
       
        _testCases = testCases;
    }
    
    public TestResult Run()
    {
        var caseResults = TestCases();
        return new TestResult(_method, caseResults.ToArray());
    }

    private IEnumerable<TestCaseResult> TestCases()
    {
        foreach (var testCase in _testCases)
        {
            object? res = null; // This is necessary to handle error 'Local variable 'res' might not be initialized before accessing'
            Exception? exceptionThrown = null;
            
            try
            {
                res = _method.Invoke(testCase.TargetActivator?.Invoke(), testCase.Parameters) ?? throw new Exception("Method Returned Null Value");
            }
            catch (TargetInvocationException e)
            {
                exceptionThrown = e.InnerException;
            }

            if (exceptionThrown != null)
            {
                yield return new TestCaseResult(testCase.Parameters, testCase.ExpectedResult,
                    new ExceptionThrownMethodResult(exceptionThrown));
            }
            else
            {
                yield return new TestCaseResult(testCase.Parameters, testCase.ExpectedResult,
                    new ReturnValueMethodResult(res!));
            }
        }
    }
}