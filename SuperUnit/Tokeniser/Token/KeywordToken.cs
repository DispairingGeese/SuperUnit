namespace SuperUnit.Tokeniser.Token;

public class KeywordToken : Token
{
    public Keyword Keyword { get; }
    
    public KeywordToken(Keyword keyword, int line, int column) : base(line, column)
    {
        Keyword = keyword;
    }
    
    public override string ToString()
    {
        return $"[ Keyword Token: {
            Keyword switch {
                Keyword.Using => "Using",
                Keyword.TestGroup => "TestGroup",
                Keyword.Do => "Do",
                Keyword.Expect => "Expect",
                Keyword.Case => "Case",
                Keyword.New => "New",
                Keyword.Throws => "Throws",
                Keyword.On => "On",
                Keyword.Namespace => "Namespace",
                _ => throw new ArgumentOutOfRangeException(),
            }
        } ]";
    }
}

public enum Keyword
{
    Using,
    TestGroup,
    Do,
    Expect,
    Case,
    New,
    Throws,
    On,
    Namespace,
}