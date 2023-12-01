namespace SuperUnit.Tokeniser.Token;

public class SymbolToken : Token
{
    public Symbol Symbol { get; }

    public SymbolToken(Symbol symbol, int line, int column) : base(line, column)
    {
        Symbol = symbol;
    }

    public override string ToString()
    {
        return $"[ Symbol Token: {
            Symbol switch {
                Symbol.Arrow => "Arrow",
                Symbol.Comma => "Comma",
                Symbol.LParen => "LParen",
                Symbol.RParen => "RParen",
                Symbol.SemiColon => "SemiColon",
                Symbol.Colon => "Colon",
                Symbol.QuestionMark => "QuestionMark",
                Symbol.Tilde => "Tilde",
                _ => throw new ArgumentOutOfRangeException(),
            }
        } ]";
    }
}

public enum Symbol
{
    Arrow,
    SemiColon,
    LParen,
    RParen,
    Comma,
    Colon,
    QuestionMark,
    Tilde,
}