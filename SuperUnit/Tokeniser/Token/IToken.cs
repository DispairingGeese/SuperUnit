namespace SuperUnit.Tokeniser.Token;

public abstract class Token
{
    public int Line { get; }
    
    public int Column { get; }

    protected Token(int line, int column)
    {
        Line = line;
        Column = column;
    }
}