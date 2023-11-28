namespace SuperUnit.Tests.Results.MethodResult;

public interface IMethodResult
{
    public bool Matches(IMethodResult other);
}