using SuperUnit.Tests;

namespace SuperUnit;

public static class TestPrinter
{
    public static void PrintTests(IEnumerable<TestGroup> tests)
    {
        Console.WriteLine("");
        
        var allPassing = true;
        
        foreach (var testGroup in tests)
        {
            if (testGroup.Tests.Length == 0)
                continue;

            allPassing &= PrintTest(testGroup);
            
            Console.WriteLine(allPassing ? $"Test Group {testGroup.Name} Passing" : $"One or More tests in Test Group {testGroup.Name} Failing");
            Console.WriteLine("");
        }

        if (allPassing)
        {
            Console.WriteLine("---------------------------");
            Console.WriteLine("---> All Tests Passing <---");
            Console.WriteLine("---------------------------");
        }
    }

    public static bool PrintTest(TestGroup testGroup)
    {
        Console.WriteLine($"----------------{new string('-', testGroup.Name.Length)}-----");
        Console.WriteLine($"---> Test Group {testGroup.Name} <---");
        Console.WriteLine($"----------------{new string('-', testGroup.Name.Length)}-----");
        Console.WriteLine("");

        var testGroupPassing = true;

        foreach (var test in testGroup.Tests)
        {
            var result = test.Run();
            
            var failingCaseResults = result.CaseResults.Where(caseResult => !caseResult.IsSuccessful).ToArray();

            if (failingCaseResults.Length == 0)
            {
                Console.WriteLine($"Test {result.Method} Passing");
                continue;
            }
            
            Console.WriteLine($"One or more tests in Test {result.Method} Failing");

            foreach (var testCaseResult in failingCaseResults)
            {
                Console.WriteLine($"{result.Method.DeclaringType?.Name ?? "UndefinedType"}->{result.Method.Name}{testCaseResult.Parameters.Aggregate("(", (s, o) => s + o.ToTypedString() + ',').TrimEnd(',')})" +
                                  $"failed, expected {testCaseResult.ExpectedResult}, got {testCaseResult.MethodResult}");
            }
        }

        return testGroupPassing;
    }
}