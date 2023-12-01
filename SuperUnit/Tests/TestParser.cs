using System.Reflection;
using System.Security;
using SuperUnit.Tests.Results.ExpectedResult.Return;
using SuperUnit.Tests.Results.ExpectedResult.Thrown;
using SuperUnit.Tests.Results.MethodResult;
using SuperUnit.Tokeniser.Token;

namespace SuperUnit.Tests;

public class TestParser
{
    private static readonly Dictionary<string, Type> TypeAliases = new()
    {
        { "bool", typeof(bool) },
        { "byte", typeof(byte) },
        { "sbyte", typeof(sbyte) },
        { "char", typeof(char) },
        { "decimal", typeof(decimal) },
        { "double", typeof(double) },
        { "float", typeof(float) },
        { "int", typeof(int) },
        { "uint", typeof(uint) },
        { "nint", typeof(nint) },
        { "nuint", typeof(nuint) },
        { "long", typeof(long) },
        { "ulong", typeof(ulong) },
        { "short", typeof(short) },
        { "ushort", typeof(ushort) },
        { "object", typeof(object) },
        { "string", typeof(string) },
    };
    
    private readonly Token[] _tokens;

    private Token? _currentToken;
    private int _pointer;

    private readonly List<Assembly> _loadedAssemblies = new() { typeof(Int32).Assembly };
    private readonly List<string> _includedNamespaces = new() { "" };

    private Dictionary<string, List<Test>> tests = new();
    private List<Test> currentTestGroupList = new();
    
    public TestParser(Token[] tokens)
    {
        _tokens = tokens;
        _pointer = -1;
    }

    public IEnumerable<TestGroup> Parse()
    {
        var tests = ParseTests();

        foreach (var key in tests.Keys)
        {
            yield return new TestGroup(key, tests[key].ToArray());
        }
    }

    private Dictionary<string, List<Test>> ParseTests()
    {
        Advance();
        SetTestGroup("");
        
        while (_currentToken is not null)
        {
            if (_currentToken is KeywordToken keywordToken)
            {
                switch (keywordToken.Keyword)
                {
                    case Keyword.Using:
                        Advance();
                        HandleUsing();
                        continue;
                    
                    case Keyword.TestGroup:
                        Advance();
                        var currentTestGroupName = ParseTestGroup();
                        SetTestGroup(currentTestGroupName);
                        continue;

                    case Keyword.Do:
                        Advance();
                        
                        currentTestGroupList.Add(ParseTest());
                        continue;
                }
            }
            
            throw CreateParserException("Unexpected token");
        }

        return tests;
    }

    private void HandleUsing()
    {
        if (_currentToken is LiteralToken<string> stringToken)
        {
            try
            {
                var assembly = Assembly.LoadFrom(stringToken.Value);
                _loadedAssemblies.Add(assembly);
            }
            catch (ArgumentException)
            {
                throw CreateParserException("Empty Assembly Path");
            }
            catch (FileNotFoundException)
            {
                throw CreateParserException("Assembly does not exist");
            }
            catch (FileLoadException)
            {
                throw CreateParserException("Assembly was found but could not be loaded");
            }
            catch (BadImageFormatException)
            {
                throw CreateParserException(
                    "Assembly was found but is invalid in this context (see BadImageFormatException)");
            }
            catch (SecurityException)
            {
                throw CreateParserException(
                    "System.Net.WebPermission was required to load assembly");
            }
            catch (PathTooLongException)
            {
                throw CreateParserException(
                    "Assembly Path was longer than the system-defined maximum length");
            }

            Advance();

            if (_currentToken is not SymbolToken { Symbol: Symbol.SemiColon })
            {
                throw CreateParserException("Expected semicolon after assembly inclusion");
            }

            Advance();
            return;
        }

        if (_currentToken is KeywordToken { Keyword: Keyword.Namespace })
        {
            Advance();

            if (_currentToken is not IdentifierToken namespaceIdentifier)
            {
                throw CreateParserException("Expected namespace name");
            }

            _includedNamespaces.Add(namespaceIdentifier.Identifier);

            Advance();

            if (_currentToken is not SymbolToken { Symbol: Symbol.SemiColon })
            {
                throw CreateParserException("Expected semicolon after namespace inclusion");
            }

            Advance();
            return;
        }

        throw CreateParserException("Expected assembly name after using statement");
    }

    private string ParseTestGroup()
    {
        string testGroup;
        
        if (_currentToken is IdentifierToken identifierToken)
        {
            testGroup = identifierToken.Identifier;
        }
        else
        {
            throw CreateParserException("Expected test name after test statement");
        }

        Advance();

        if (_currentToken is not SymbolToken { Symbol: Symbol.SemiColon })
        {
            throw CreateParserException("Expected semicolon after test declaration");
        }

        Advance();
        return testGroup;
    }

    private Test ParseTest()
    {
        if (_currentToken is not IdentifierToken typeNameIdentifier)
        {
            throw CreateParserException("Expected Type Name");
        }

        var typeName = typeNameIdentifier.Identifier;
        var type = GetTypeFromLoadedAssemblies(typeName);
        
        Advance();

        if (_currentToken is not SymbolToken { Symbol: Symbol.Arrow })
        {
            throw CreateParserException("Expected '->'");
        }

        Advance();

        if (_currentToken is not IdentifierToken methodIdentifier)
        {
            throw CreateParserException("Expected Method Name after 'do'");
        }

        var methodName = methodIdentifier.Identifier;

        Advance();

        Type[]? staticTypeParameters = null;
        
        if (_currentToken is SymbolToken { Symbol: Symbol.LParen })
        {
            staticTypeParameters = ParseTypeParameters();
            Advance();
        }

        if (_currentToken is not SymbolToken { Symbol: Symbol.Colon })
        {
            throw CreateParserException("Expected :");
        }

        Advance();

        var cases = new List<TestCase>();

        Func<object>? currentTestTargetActivator = null;

        while (true)
        {
            if (_currentToken is KeywordToken { Keyword: Keyword.Case })
            {
                Advance();

                var @params = ParseCsv().ToArray();

                Advance();

                if (_currentToken is KeywordToken currentKeywordToken)
                {
                    switch (currentKeywordToken.Keyword)
                    {
                        case Keyword.Expect:
                            Advance();

                            cases.Add(new TestCase(currentTestTargetActivator, @params, new EqualityTestExpectedResult(ParseValue())));
                            break;

                        case Keyword.Throws:
                            Advance();

                            var thrownObject = ParseValue();

                            cases.Add(new TestCase(currentTestTargetActivator, @params, new ExceptionThrownTestExpectedResult(thrownObject)));

                            break;

                        case Keyword.Using:
                        case Keyword.TestGroup:
                        case Keyword.Do:
                        case Keyword.Case:
                        case Keyword.New:
                        default:
                            throw CreateParserException("Expected test result");
                    }
                }
                else
                {
                    throw CreateParserException("Expected test result");
                }

                Advance();

                if (_currentToken is SymbolToken { Symbol: Symbol.Comma })
                {
                    Advance();
                    continue;
                }

                if (_currentToken is SymbolToken { Symbol: Symbol.SemiColon })
                {
                    Advance();
                    break;
                }

                throw CreateParserException("Unexpected Token");
            }
            
            if (_currentToken is KeywordToken { Keyword: Keyword.On })
            {
                Advance();
                currentTestTargetActivator = ParseValueActivator(typeName);
                
                Advance();

                if (_currentToken is not SymbolToken { Symbol: Symbol.Colon })
                {
                    throw CreateParserException("Expected ':'");
                }
                
                Advance();
            }
            else
            {
                throw CreateParserException("Expected test case after ','");
            }
        }

        var method = type.GetMethod(methodName, staticTypeParameters ?? InferParameterTypes(cases[0].Parameters));

        if (method is null)
        {
            throw CreateParserException($"Cannot find method '{methodName}' on type {type}", methodIdentifier);
        }

        return new Test(method, cases.ToArray());
    }

    private Type[] ParseTypeParameters()
    {
        try
        {
            return ParseCsv().Cast<Type>().ToArray();
        }
        catch (InvalidCastException)
        {
            throw CreateParserException("Type Expected");
        }
    }

    private IEnumerable<object> ParseCsv()
    {
        if (_currentToken is not SymbolToken { Symbol: Symbol.LParen })
        {
            throw CreateParserException("Expected '('");
        }
        
        Advance();

        while (_currentToken is not SymbolToken { Symbol: Symbol.RParen })
        {
            yield return ParseValue();

            Advance();

            if (_currentToken is not (SymbolToken { Symbol: Symbol.RParen } or SymbolToken { Symbol: Symbol.Comma }))
            {
                throw CreateParserException("Expected ) or ,");
            }

            if (_currentToken is SymbolToken { Symbol: Symbol.Comma })
                Advance();
        }
    }

    private object ParseValue(string? inferredTypeName = null)
    {
        return ParseValueActivator(inferredTypeName).Invoke();
    }

    private Func<object> ParseValueActivator(string? inferredTypeName = null)
    {
        if (_currentToken is IValueToken valueToken)
        {
            return () => valueToken.GetValue();
        }

        if (_currentToken is IdentifierToken typeIdentifierToken)
        {
            var type = GetTypeFromLoadedAssemblies(typeIdentifierToken.Identifier);
            return () => type;
        }

        if (_currentToken is KeywordToken { Keyword: Keyword.New })
        {
            Advance();

            string typeName;

            if (_currentToken is IdentifierToken identifierToken)
            {
                Advance();
                typeName = identifierToken.Identifier;
            }
            else if (inferredTypeName is not null)
            {
                typeName = inferredTypeName;
            }
            else
            {
                throw CreateParserException("Type Name Expected");
            }

            var constructorParams = ParseCsv().ToArray();
            var type = typeof(object).Assembly.GetType(typeName) ?? GetTypeFromLoadedAssemblies(typeName);
            
            var constructor = type.GetConstructor(InferParameterTypes(constructorParams));

            if (constructor is null)
            {
                throw CreateParserException($"Cannot get constructor on type {type} with given parameters");
            }
            
            return () => constructor.Invoke(constructorParams);
        }
        
        throw CreateParserException("Argument expected");
    }

    private Type GetTypeFromLoadedAssemblies(string typeName)
    {
        if (TypeAliases.TryGetValue(typeName, out var type))
        {
            return type;
        }
        
        foreach (var loadedAssembly in _loadedAssemblies)
        {
            try
            {
                Type? result = null;
                
                foreach (var includedNamespace in _includedNamespaces)
                {
                    result = loadedAssembly.GetType($"{includedNamespace}.{typeName}");

                    if (result != null)
                        break;
                }
                
                if (result is null)
                    continue;

                return result;
            }
            catch (ArgumentException)
            {
                throw CreateParserException($"Invalid Type Name '{typeName}'");
            }
        }

        throw CreateParserException($"Type '{typeName}' was not found (have you loaded the correct assembly?)");
    }

    private void SetTestGroup(string groupName)
    {
        if (tests.TryGetValue(groupName, out _))
        {
            throw CreateParserException($"Cannot redefine already existing test group '{groupName}'");
        }

        tests.Add(groupName, new List<Test>());
        currentTestGroupList = tests[groupName];
    }
    
    private ParserException CreateParserException(string message, Token? referencePoint = null)
    {
        return new ParserException(message, (referencePoint ?? _currentToken) ?? new NullToken());
    }

    private Type[] InferParameterTypes(object[] @params)
    {
        return @params.Select(parameter => parameter.GetType()).ToArray();
    }

    private void Advance()
    {
        _pointer += 1;
        _currentToken = _pointer >= _tokens.Length ? null : _tokens[_pointer];
    }
}