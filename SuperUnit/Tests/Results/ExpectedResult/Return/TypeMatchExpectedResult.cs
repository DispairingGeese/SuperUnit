using SuperUnit.Tests.Results.MethodResult;
using Xunit;
using Xunit.Sdk;

namespace SuperUnit.Tests.Results.ExpectedResult.Return;

public class TypeMatchExpectedResult : ITestExpectedResult
{
    public Type ExpectedType { get; }

    public TypeMatchExpectedResult(Type expectedType)
    {
        ExpectedType = expectedType;
    }

    public bool Test(IMethodResult actual)
    {
        if (actual is not ReturnValueMethodResult returnValueMethodResult)
            return false;

        try
        {
            Assert.Equal(ExpectedType, returnValueMethodResult.ReturnValue.GetType());
        }
        catch (EqualException)
        {
            return false;
        }
        
        return true;
    }

    public override string ToString()
    {
        return $"object of type {ExpectedType.Name} returned";
    }
}