namespace SuperUnit.Tests.Results.MethodResult;

public class ExceptionThrownMethodResult : IMethodResult
{
    public object ThrownException { get; }
    
    public ExceptionThrownMethodResult(object thrownException)
    {
        ThrownException = thrownException;
    }
    
    public override string ToString()
    {
        return $"{ThrownException.ToTypedString()} thrown";
    }
}