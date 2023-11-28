namespace SuperUnit.Tests;

public class TestGroup
{
    public string Name { get; }
    
    public Test[] Tests { get; }

    public TestGroup(string name, Test[] tests)
    {
        Name = name;
        Tests = tests;
    }
}