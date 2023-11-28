namespace SuperUnit.Tokeniser.Token;

public class LiteralToken<T> : Token, IValueToken where T : notnull
{    
    public T Value { get; }

    public LiteralToken(T value, int line, int column) : base(line, column)
    {
        Value = value;
    }

    public override string ToString()
    {
        return $"[ LiteralToken<{typeof(T).Name}>: {Value} ]";
    }

    public object GetValue()
    {
        return Value;
    }
}