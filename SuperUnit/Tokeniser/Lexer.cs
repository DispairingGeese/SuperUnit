using SuperUnit.Tokeniser.Token;

namespace SuperUnit.Tokeniser;

public class Lexer
{
    private const string IdentifierStartCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_";
    private const string IdentifierBodyCharacters = $"{IdentifierStartCharacters}{Numbers}.";

    private const string WhiteSpace = " \n\t";
    
    private const string Numbers = "1234567890";
    private const string NumericPostfixes = $"{IntegerPostfixes}{FloatPostfixes}b";
    private const string IntegerPostfixes = "iuls";
    private const string FloatPostfixes = "fd";

    private static readonly Dictionary<string, Keyword> Keywords = new()
    {
        { "using", Keyword.Using },
        { "test_group", Keyword.TestGroup },
        { "do", Keyword.Do },
        { "expect", Keyword.Expect },
        { "case", Keyword.Case },
        { "new", Keyword.New },
        { "throws", Keyword.Throws },
        { "on", Keyword.On },
        { "namespace", Keyword.Namespace },
        { "instanceof", Keyword.InstanceOf },
    };

    private static readonly Dictionary<char, Symbol> Symbols = new()
    {
        { ';', Symbol.SemiColon },
        { '(', Symbol.LParen },
        { ')', Symbol.RParen },
        { ',', Symbol.Comma },
        { ':', Symbol.Colon },
        { '~', Symbol.Tilde },
    };

    private static readonly Dictionary<string, Symbol> BinarySymbols = new()
    {
        { "->", Symbol.Arrow },
    };

    private readonly string _chars;

    private char? _currentChar;
    private int _pointer;

    private int _currentColumn;
    private int _currentLine;

    private int _currentTokenColumn;
    private int _currentTokenLine;
    
    public Lexer(string chars)
    {
        _chars = chars.ReplaceLineEndings("\n");
        _pointer = -1;

        _currentColumn = 0;
        _currentLine = 1;
    }

    public IEnumerable<Token.Token> Lex()
    {
        Advance();
        
        while (_currentChar.HasValue)
        {
            _currentTokenColumn = _currentColumn;
            _currentTokenLine = _currentLine;

            if (WhiteSpace.Contains(_currentChar.Value))
            {
                Advance();
                continue;
            }
            
            if (_currentChar == ':' && Peek() == ':')
            {
                HandleLineComment();
                continue;
            }

            if (_currentChar == '"')
            {
                yield return LexString();
                continue;
            }
            
            if (_currentChar == '&')
            {
                Advance();

                yield return _currentChar switch
                {
                    '"' => LexString(true),
                    '\'' => LexChar(true),
                    _ => throw CreateLexerException("Expected Raw Text Literal")
                };

                continue;
            }
            
            if (_currentChar == '\'')
            {
                yield return LexChar();
                continue;
            }

            if (IdentifierStartCharacters.Contains(_currentChar.Value))
            {
                yield return LexIdentifier();
                continue;
            }

            if (Numbers.Contains(_currentChar.Value) || (_currentChar == '-' && Numbers.Contains(Peek() ?? '\0')))
            {
                yield return LexNumeric();
                continue;
            }
            
            if (Symbols.TryGetValue(_currentChar.Value, out var symbol))
            {
                yield return new SymbolToken(symbol, _currentTokenLine, _currentTokenColumn);
                Advance();
                continue;
            }
            
            if (BinarySymbols.TryGetValue($"{_currentChar.Value}{Peek() ?? '\0'}", out var binarySymbol))
            {
                yield return new SymbolToken(binarySymbol, _currentTokenLine, _currentTokenColumn);
                Advance();
                Advance();
                continue;
            }

            throw CreateLexerException($"Unexpected character '{_currentChar}'");
        }
    }
    
    private char? Peek(int distance = 1)
    {
        return _pointer + distance >= _chars.Length ? null : _chars[_pointer + distance];
    }
    
    private void Advance()
    {
        _pointer += 1;
        _currentChar = Peek(0);

        _currentColumn += 1;

        if (_currentChar == '\n')
        {
            _currentColumn = 0;
            _currentLine += 1;
        }
    }
    
    private void HandleLineComment()
    {
        while (true)
        {
            Advance();
            
            if (_currentChar is null or '\n')
            {
                Advance();
                break;
            }
        }
    }

    private Token.Token LexString(bool ignoreEscapeSequences = false)
    {
        var identifier = "";

        Advance();
        
        while (true)
        {
            var @char = LexLiteralCharacter();

            if (@char == '"')
            {
                break;
            }

            if (@char == '\\' && !ignoreEscapeSequences)
            {
                @char = HandleEscapeSequence();
            }
            
            identifier += @char;
        }
        
        return new LiteralToken<string>(identifier, _currentTokenLine, _currentTokenColumn);
    }
    
    private Token.Token LexChar(bool ignoreEscapeSequences = false)
    {
        Advance();
     
        var @char = LexLiteralCharacter();
        
        if (@char == '\'')
        {
            throw CreateLexerException("Empty Character Literal");
        }

        if (@char == '\\' && !ignoreEscapeSequences)
        {
            @char = HandleEscapeSequence();
        }
        
        if (_currentChar != '\'')
        {
            throw CreateLexerException("Expected \"'\"");
        }

        Advance();

        return new LiteralToken<char>(@char, _currentTokenLine, _currentTokenColumn);
    }

    private char LexLiteralCharacter()
    {
        if (_currentChar == null)
        {
            throw CreateLexerException("Reached End of file when parsing literal");
        }
        
        if (_currentChar == '\n')
        {
            throw CreateLexerException("Reached End of line when parsing literal");
        }

        var @char = _currentChar;
        Advance();
        return @char.Value;
    }

    private char HandleEscapeSequence()
    {
        var nextChar = LexLiteralCharacter();
        
        return nextChar switch
        {
            '\\' => '\\',
            'n' => '\n',
            't' => '\t',
            '\'' => '\'',
            '\"' => '\"',
            _ => throw CreateLexerException($"Invalid escape sequence \'\\{nextChar}\'"),
        };
    }

    private Token.Token LexIdentifier()
    {
        var identifier = "";
        
        while (true)
        {
            identifier += _currentChar;
            Advance();
            
            if (_currentChar == null || !IdentifierBodyCharacters.Contains(_currentChar.Value))
            {
                break;
            }
        }

        if (Keywords.TryGetValue(identifier, out var keyword))
        {
            return new KeywordToken(keyword, _currentTokenLine, _currentTokenColumn);
        }

        if (identifier == "true")
        {
            return new LiteralToken<bool>(true, _currentTokenLine, _currentColumn);
        }
        
        if (identifier == "false")
        {
            return new LiteralToken<bool>(false, _currentTokenLine, _currentColumn);
        }

        return new IdentifierToken(identifier, _currentTokenLine, _currentTokenColumn);
    }

    private Token.Token LexNumeric()
    {
        var literal = "";
        char? postfix = null;

        var hasPoint = false;
        
        while (true)
        {
            literal += _currentChar;
            Advance();
            
            if (_currentChar is null or ' ')
            {
                break;
            }

            if (NumericPostfixes.Contains(_currentChar.Value))
            {
                postfix = _currentChar;
                
                if (postfix == 'u' && literal[0] == '-')
                {
                    throw CreateLexerException("Unsigned int literal cannot be negative");
                }
                
                if (postfix == 'b' && literal[0] == '-')
                {
                    throw CreateLexerException("Byte literal cannot be negative");
                }
                
                if (postfix == 'b' && hasPoint)
                {
                    throw CreateLexerException("Byte literal cannot have decimal point");
                }

                if (hasPoint && IntegerPostfixes.Contains(postfix.Value))
                {
                    throw CreateLexerException("Integer literal cannot have decimal point");
                }

                Advance();

                break;
            }

            if (Numbers.Contains(_currentChar.Value))
            {
                continue;
            }

            if (_currentChar.Value == '.')
            {
                if (hasPoint)
                    throw CreateLexerException("Unexpected '.'");
                
                hasPoint = true;
                continue;
            }

            break;
        }

        if (!postfix.HasValue)
        {
            if (hasPoint)
            {
                // float.Parse can be safely used as parsing already handles illegal characters (hopefully)
                var floatValue = float.Parse(literal);
                return new LiteralToken<float>(floatValue, _currentTokenLine, _currentTokenColumn);
            }
            else
            {
                // int.Parse can be safely used. See above.
                var intValue = int.Parse(literal);
                return new LiteralToken<int>(intValue, _currentTokenLine, _currentTokenColumn);
            }
        }
        
        switch (postfix)
        {
            case 'i':
                var intValue = int.Parse(literal);
                return new LiteralToken<int>(intValue, _currentTokenLine, _currentTokenColumn);
            case 'u':
                var uintValue = uint.Parse(literal);
                return new LiteralToken<uint>(uintValue, _currentTokenLine, _currentTokenColumn);
            case 'l':
                var longValue = long.Parse(literal);
                return new LiteralToken<long>(longValue, _currentTokenLine, _currentTokenColumn);
            case 's':
                var shortValue = short.Parse(literal);
                return new LiteralToken<short>(shortValue, _currentTokenLine, _currentTokenColumn);
            
            case 'f':
                var floatValue = float.Parse(literal);
                return new LiteralToken<float>(floatValue, _currentTokenLine, _currentTokenColumn);
            case 'd':
                var doubleValue = double.Parse(literal);
                return new LiteralToken<double>(doubleValue, _currentTokenLine, _currentTokenColumn);
            
            case 'b':
                var byteValue = (byte)uint.Parse(literal);
                return new LiteralToken<byte>(byteValue, _currentTokenLine, _currentTokenColumn);
            
            default:
                throw new ArgumentOutOfRangeException(nameof(postfix));
        }
    }

    private LexerException CreateLexerException(string message)
    {
        return new LexerException(message, _currentLine, _currentColumn);
    }
}