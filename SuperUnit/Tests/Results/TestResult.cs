using System.Reflection;

namespace SuperUnit.Tests.Results;

public readonly struct TestResult
{
    public MethodInfo Method { get; }
    
    public TestCaseResult[] CaseResults { get; }
    
    public TestResult(MethodInfo method, TestCaseResult[] caseResults)
    {
        Method = method;
        CaseResults = caseResults;
    }
}