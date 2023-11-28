namespace SuperUnit.Tests.Results.MethodResult;

public class ExceptionThrownMethodResult : IMethodResult
{
    public Type ThrownExceptionType { get; }
    
    public ExceptionThrownMethodResult(Type thrownExceptionType)
    {
        ThrownExceptionType = thrownExceptionType;
    }
    
    public override string ToString()
    {
        return $"{ThrownExceptionType.Name} thrown";
    }

    public bool Matches(IMethodResult other)
    {
        return other is ExceptionThrownMethodResult exceptionThrownMethodResult
               && exceptionThrownMethodResult.ThrownExceptionType == ThrownExceptionType;
    }
}