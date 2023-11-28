namespace SuperUnit.Tokeniser.Token;

public class IdentifierToken : Token
{
    public string Identifier { get; }
    
    public IdentifierToken(string identifier, int line, int column) : base(line, column)
    {
        Identifier = identifier;
    }

    public override string ToString()
    {
        return $"[ Identifier Token: {Identifier} ]";
    }
}