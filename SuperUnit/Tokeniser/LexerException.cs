namespace SuperUnit.Tokeniser;

public class LexerException : Exception
{
    public LexerException(string message, int line, int column) : base($"Lexer Error: {message} (line {line} column {column})")
    {
    }
}