using SuperUnit.Tokeniser.Token;

namespace SuperUnit.Tests;

public class ParserException : Exception
{
    public ParserException(string message, Token referencePoint)
        : base($"Encountered Parser Error: {message} (line {referencePoint.Line}, column {referencePoint.Column})")
    {
    }
}