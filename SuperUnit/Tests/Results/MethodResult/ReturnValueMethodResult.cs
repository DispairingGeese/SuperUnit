namespace SuperUnit.Tests.Results.MethodResult;

public class ReturnValueMethodResult : IMethodResult
{
    public object ReturnValue { get; }
    
    public ReturnValueMethodResult(object returnValue)
    {
        ReturnValue = returnValue;
    }

    public override string ToString()
    {
        return $"{ReturnValue.ToTypedString()} returned";
    }
}